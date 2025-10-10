export interface FieldOption {
  value: string;
  label: string;
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

export interface TemplateSection {
  id: string;
  title: string;
  description?: string;
  fields: FormField[];
  aiGenerated?: boolean;
}

export interface Template {
  tripType: string;
  title: string;
  sections: TemplateSection[];
}

export interface FormData {
  [key: string]: any;
}
