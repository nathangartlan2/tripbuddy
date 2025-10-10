import React, { useState } from "react";
import { Template, TemplateSection, FormData } from "../types";
import { FieldRenderer } from "./FieldRenderer";

interface FormRendererProps {
  template: Template;
  onSubmit: (formData: FormData) => void;
}

export const FormRenderer: React.FC<FormRendererProps> = ({
  template,
  onSubmit,
}) => {
  const [formData, setFormData] = useState<FormData>({});

  const handleFieldChange = (name: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(formData);
  };

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-lg shadow-md">
      <h1 className="text-3xl font-bold text-gray-800 mb-2">
        {template.title}
      </h1>
      <p className="text-gray-600 mb-6">Trip Type: {template.tripType}</p>

      <form onSubmit={handleSubmit} className="space-y-8">
        {template.sections.map((section: TemplateSection) => (
          <div
            key={section.id}
            className="border border-gray-200 rounded-lg p-6"
          >
            <h2 className="text-xl font-semibold text-gray-800 mb-4">
              {section.title}
            </h2>
            {section.description && (
              <p className="text-gray-600 mb-4">{section.description}</p>
            )}

            <div className="space-y-4">
              {section.fields.map((field) => (
                <FieldRenderer
                  key={field.name}
                  field={field}
                  value={formData[field.name]}
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
          >
            Generate Trip Plan
          </button>
        </div>
      </form>

      {/* Debug: Show current form data */}
      <div className="mt-8 p-4 bg-gray-50 rounded-lg">
        <h3 className="text-lg font-medium text-gray-800 mb-2">
          Form Data (Debug)
        </h3>
        <pre className="text-sm text-gray-600 overflow-auto">
          {JSON.stringify(formData, null, 2)}
        </pre>
      </div>
    </div>
  );
};
