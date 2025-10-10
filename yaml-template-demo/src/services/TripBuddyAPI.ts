// API client for TripBuddy session management

export interface SessionRequest {
  userId?: string;
  tripType: string;
  initialTemplate: string;
}

export interface SessionResponse {
  sessionId: string;
  template: Template;
  context: Record<string, any>;
  formData: Record<string, any>;
  guidance: string;
  expiresAt: string;
  lastUpdated: string;
}

export interface ContextUpdateRequest {
  fieldUpdates: Record<string, any>;
  triggerLLM?: boolean;
  currentProgress?: number;
}

export interface ContextUpdateResponse {
  guidance: string;
  templateUpdates?: TemplateUpdates;
  suggestions: string[];
  warnings: string[];
}

export interface TemplateUpdates {
  newSections: TemplateSection[];
  modifiedSections: string[];
  hiddenSections: string[];
}

export interface Template {
  tripType: string;
  title: string;
  sections: TemplateSection[];
}

export interface TemplateSection {
  id: string;
  title: string;
  description?: string;
  fields: FormField[];
  aiGenerated?: boolean;
}

export interface FormField {
  name: string;
  type: "text" | "number" | "select" | "checkbox";
  label: string;
  required?: boolean;
  placeholder?: string;
  min?: number;
  max?: number;
  options?: FieldOption[];
}

export interface FieldOption {
  value: string;
  label: string;
}

export interface FieldSuggestionRequest {
  fieldName: string;
  currentValue?: string;
  formContext: Record<string, any>;
}

export interface FieldSuggestionResponse {
  fieldName: string;
  suggestions: string[];
  explanation: string;
}

export interface AIGuidanceRequest {
  trigger: string;
  context: {
    changedField?: string;
    newValue?: any;
    formProgress: number;
  };
}

export interface AIGuidanceResponse {
  guidance: string;
  actionItems: string[];
  dynamicContent?: {
    addSections: TemplateSection[];
    modifyFields: FormField[];
    showWarnings: string[];
  };
}

export interface ValidationRequest {
  section: string;
  data: Record<string, any>;
}

export interface ValidationResponse {
  valid: boolean;
  issues: ValidationIssue[];
  recommendations: string[];
}

export interface ValidationIssue {
  field: string;
  severity: "error" | "warning" | "info";
  message: string;
  suggestion?: string;
}

export interface SuggestionsResponse {
  suggestions: FieldSuggestion[];
  autoComplete: string[];
  warnings: string[];
}

export interface FieldSuggestion {
  value: string;
  label: string;
  reason: string;
  confidence: number;
}

export class TripBuddyAPI {
  private baseUrl: string;
  private defaultHeaders: Record<string, string>;

  constructor(baseUrl: string = "https://localhost:5001/api") {
    this.baseUrl = baseUrl;
    this.defaultHeaders = {
      "Content-Type": "application/json",
    };
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;

    const config: RequestInit = {
      ...options,
      headers: {
        ...this.defaultHeaders,
        ...options.headers,
      },
    };

    const response = await fetch(url, config);

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(
        errorData.error || `HTTP ${response.status}: ${response.statusText}`
      );
    }

    return response.json();
  }

  // Session Management
  async createSession(request: SessionRequest): Promise<SessionResponse> {
    return this.request<SessionResponse>("/sessions", {
      method: "POST",
      body: JSON.stringify(request),
    });
  }

  async getSession(sessionId: string): Promise<SessionResponse> {
    return this.request<SessionResponse>(`/sessions/${sessionId}`);
  }

  async deleteSession(sessionId: string): Promise<void> {
    await this.request<void>(`/sessions/${sessionId}`, {
      method: "DELETE",
    });
  }

  async updateContext(
    sessionId: string,
    request: ContextUpdateRequest
  ): Promise<ContextUpdateResponse> {
    return this.request<ContextUpdateResponse>(
      `/sessions/${sessionId}/context`,
      {
        method: "PATCH",
        body: JSON.stringify(request),
      }
    );
  }

  // AI Integration
  async getAIGuidance(
    sessionId: string,
    request: AIGuidanceRequest
  ): Promise<AIGuidanceResponse> {
    return this.request<AIGuidanceResponse>(
      `/sessions/${sessionId}/ai/guidance`,
      {
        method: "POST",
        body: JSON.stringify(request),
      }
    );
  }

  async getFieldSuggestion(
    sessionId: string,
    request: FieldSuggestionRequest
  ): Promise<FieldSuggestionResponse> {
    return this.request<FieldSuggestionResponse>(
      `/sessions/${sessionId}/ai/field-suggestion`,
      {
        method: "POST",
        body: JSON.stringify(request),
      }
    );
  }

  async validateData(
    sessionId: string,
    request: ValidationRequest
  ): Promise<ValidationResponse> {
    return this.request<ValidationResponse>(
      `/sessions/${sessionId}/ai/validate`,
      {
        method: "POST",
        body: JSON.stringify(request),
      }
    );
  }

  async getSuggestions(
    sessionId: string,
    fieldName: string,
    currentValue?: string
  ): Promise<SuggestionsResponse> {
    const params = new URLSearchParams();
    if (currentValue) {
      params.append("currentValue", currentValue);
    }

    const query = params.toString() ? `?${params.toString()}` : "";
    return this.request<SuggestionsResponse>(
      `/sessions/${sessionId}/suggestions/${fieldName}${query}`
    );
  }

  // Template Management
  async getTemplate(templateId: string): Promise<Template> {
    return this.request<Template>(`/templates/${templateId}`);
  }

  async listTemplates(): Promise<{
    templates: Array<{ id: string; name: string; tripType: string }>;
  }> {
    return this.request<{
      templates: Array<{ id: string; name: string; tripType: string }>;
    }>("/templates");
  }
}

// Singleton instance
export const tripBuddyAPI = new TripBuddyAPI();

// Session management class
export class TripPlanningSession {
  private api: TripBuddyAPI;
  private sessionId: string | null = null;
  private template: Template | null = null;
  private formData: Record<string, any> = {};

  constructor(api: TripBuddyAPI = tripBuddyAPI) {
    this.api = api;
  }

  async initialize(tripType: string = "backpacking"): Promise<SessionResponse> {
    const response = await this.api.createSession({
      tripType,
      initialTemplate: `${tripType}-template-v1`,
    });

    this.sessionId = response.sessionId;
    this.template = response.template;
    this.formData = response.formData;

    return response;
  }

  async updateField(
    fieldName: string,
    value: any,
    options: { triggerLLM?: boolean; currentProgress?: number } = {}
  ): Promise<ContextUpdateResponse> {
    if (!this.sessionId) {
      throw new Error("Session not initialized");
    }

    this.formData[fieldName] = value;

    const response = await this.api.updateContext(this.sessionId, {
      fieldUpdates: { [fieldName]: value },
      triggerLLM: options.triggerLLM || false,
      currentProgress: options.currentProgress || this.calculateProgress(),
    });

    // Apply template updates if any
    if (response.templateUpdates) {
      this.applyTemplateUpdates(response.templateUpdates);
    }

    return response;
  }

  async getSmartSuggestions(
    fieldName: string,
    currentValue?: any
  ): Promise<SuggestionsResponse> {
    if (!this.sessionId) {
      throw new Error("Session not initialized");
    }

    return this.api.getSuggestions(
      this.sessionId,
      fieldName,
      currentValue?.toString()
    );
  }

  async getFieldSuggestion(
    fieldName: string,
    currentValue?: string
  ): Promise<FieldSuggestionResponse> {
    if (!this.sessionId) {
      throw new Error("No active session");
    }

    return this.api.getFieldSuggestion(this.sessionId, {
      fieldName,
      currentValue,
      formContext: this.formData,
    });
  }

  async validateSection(sectionId: string): Promise<ValidationResponse> {
    if (!this.sessionId) {
      throw new Error("No active session");
    }

    const sectionData = this.getSectionData(sectionId);
    return this.api.validateData(this.sessionId, {
      section: sectionId,
      data: sectionData,
    });
  }

  getTemplate(): Template | null {
    return this.template;
  }

  getFormData(): Record<string, any> {
    return { ...this.formData };
  }

  getSessionId(): string | null {
    return this.sessionId;
  }

  private calculateProgress(): number {
    if (!this.template) return 0;

    const totalFields = this.template.sections.reduce(
      (total, section) => total + section.fields.length,
      0
    );

    const completedFields = Object.keys(this.formData).length;
    return totalFields > 0 ? completedFields / totalFields : 0;
  }

  private applyTemplateUpdates(updates: TemplateUpdates): void {
    if (!this.template) return;

    // Add new sections
    this.template.sections.push(...updates.newSections);

    // Mark modified sections (could implement field-level updates here)
    // For now, we'll just log them
    if (updates.modifiedSections.length > 0) {
      console.log("Modified sections:", updates.modifiedSections);
    }

    // Hide sections (could implement visibility toggle)
    if (updates.hiddenSections.length > 0) {
      console.log("Hidden sections:", updates.hiddenSections);
    }
  }

  private getSectionData(sectionId: string): Record<string, any> {
    const section = this.template?.sections.find((s) => s.id === sectionId);
    if (!section) return {};

    const sectionData: Record<string, any> = {};
    section.fields.forEach((field) => {
      if (this.formData[field.name] !== undefined) {
        sectionData[field.name] = this.formData[field.name];
      }
    });

    return sectionData;
  }
}
