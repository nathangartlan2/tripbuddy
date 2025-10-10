import React, { useState } from "react";
import { Template, TemplateSection, FormData } from "../types";
import { EnhancedFieldRenderer } from "./EnhancedFieldRenderer";
import { TripPlanningSession } from "../services/TripBuddyAPI";

interface FormRendererProps {
  template: Template;
  session: TripPlanningSession;
  onSubmit: (formData: FormData) => void;
}

export const FormRenderer: React.FC<FormRendererProps> = ({
  template,
  session,
  onSubmit,
}) => {
  const [formData, setFormData] = useState<FormData>({});
  const [guidance, setGuidance] = useState<string>("");
  const [isAiThinking, setIsAiThinking] = useState(false);
  const [dynamicTemplate, setDynamicTemplate] = useState<Template>(template);

  // Fields that trigger LLM updates
  const triggerFields = [
    "destination",
    "season",
    "experience_level",
    "group_size",
    // Essential gear fields
    "shelter",
    "sleeping_bag",
    "cooking",
  ];

  const handleFieldChange = async (name: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));

    // Update session and potentially trigger AI
    const shouldTriggerLLM = triggerFields.includes(name);

    if (shouldTriggerLLM) {
      setIsAiThinking(true);
      try {
        const response = await session.updateField(name, value, {
          triggerLLM: true,
          currentProgress: calculateProgress({ ...formData, [name]: value }),
        });

        if (response.guidance) {
          setGuidance(response.guidance);
        }

        // Update template if there are new sections
        if (response.templateUpdates?.newSections.length) {
          const updatedTemplate = {
            ...dynamicTemplate,
            sections: [
              ...dynamicTemplate.sections,
              ...response.templateUpdates.newSections,
            ],
          };
          setDynamicTemplate(updatedTemplate);
        }
      } catch (error) {
        console.error("Failed to get AI guidance:", error);
      } finally {
        setIsAiThinking(false);
      }
    } else {
      // Just update the session without triggering LLM
      try {
        await session.updateField(name, value, { triggerLLM: false });
      } catch (error) {
        console.error("Failed to update session:", error);
      }
    }
  };

  const calculateProgress = (data: FormData): number => {
    const totalFields = dynamicTemplate.sections.reduce(
      (total, section) => total + section.fields.length,
      0
    );
    const completedFields = Object.keys(data).filter(
      (key) => data[key] !== undefined && data[key] !== ""
    ).length;
    return totalFields > 0 ? completedFields / totalFields : 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
      <h1 className="text-3xl font-bold text-gray-800 mb-2">
        {dynamicTemplate.title}
      </h1>
      <p className="text-gray-600 mb-6">
        Trip Type: {dynamicTemplate.tripType}
      </p>

      {/* AI Guidance Panel */}
      {(guidance || isAiThinking) && (
        <div className="mb-6 p-4 bg-blue-50 border-l-4 border-blue-400 rounded">
          <div className="flex items-center">
            {isAiThinking && (
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600 mr-2"></div>
            )}
            <h3 className="font-medium text-blue-800">
              {isAiThinking ? "AI is thinking..." : "AI Guidance"}
            </h3>
          </div>
          {guidance && !isAiThinking && (
            <p className="text-blue-700 mt-1">{guidance}</p>
          )}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-8">
        {dynamicTemplate.sections.map((section: TemplateSection) => (
          <div
            key={section.id}
            className={`border rounded-lg p-6 ${
              section.aiGenerated
                ? "border-blue-200 bg-blue-50"
                : "border-gray-200"
            }`}
          >
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-semibold text-gray-800">
                {section.title}
              </h2>
              {section.aiGenerated && (
                <span className="px-2 py-1 text-xs bg-blue-100 text-blue-800 rounded">
                  AI Generated
                </span>
              )}
            </div>
            {section.description && (
              <p className="text-gray-600 mb-4">{section.description}</p>
            )}

            <div className="space-y-4">
              {section.fields.map((field) => (
                <EnhancedFieldRenderer
                  key={field.name}
                  field={field}
                  value={formData[field.name]}
                  session={session}
                  onChange={handleFieldChange}
                />
              ))}
            </div>
          </div>
        ))}

        <div className="flex justify-center pt-6">
          <button
            type="submit"
            className="bg-blue-600 hover:bg-blue-700 text-white font-medium py-3 px-8 rounded-lg transition-colors duration-200"
            disabled={isAiThinking}
          >
            {isAiThinking ? "Processing..." : "Generate Trip Plan"}
          </button>
        </div>
      </form>

      {/* Debug: Show current form data */}
      <div className="mt-8 p-4 bg-gray-50 rounded-lg">
        <h3 className="text-lg font-medium text-gray-800 mb-2">
          Form Data (Debug)
        </h3>
        <div className="text-xs text-gray-500 mb-2">
          Progress: {Math.round(calculateProgress(formData) * 100)}% | Session:{" "}
          {session.getSessionId() || "Not connected"}
        </div>
        <pre className="text-sm text-gray-600 overflow-auto">
          {JSON.stringify(formData, null, 2)}
        </pre>
      </div>
    </div>
  );
};
