import React, { useState, useRef, useEffect } from "react";
import { FormField } from "../types";
import { FieldSuggestionPopup } from "./FieldSuggestionPopup";
import {
  TripPlanningSession,
  FieldSuggestionResponse,
} from "../services/TripBuddyAPI";

interface EnhancedFieldRendererProps {
  field: FormField;
  value: any;
  session: TripPlanningSession;
  onChange: (name: string, value: any) => void;
}

export const EnhancedFieldRenderer: React.FC<EnhancedFieldRendererProps> = ({
  field,
  value,
  session,
  onChange,
}) => {
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [suggestion, setSuggestion] = useState<FieldSuggestionResponse | null>(
    null
  );
  const [isLoadingSuggestion, setIsLoadingSuggestion] = useState(false);
  const [popupPosition, setPopupPosition] = useState({ x: 0, y: 0 });
  const fieldRef = useRef<HTMLDivElement>(null);
  const timeoutRef = useRef<number | null>(null);

  const handleFieldFocus = async (event: React.FocusEvent) => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    // Calculate popup position
    const rect = event.currentTarget.getBoundingClientRect();
    setPopupPosition({
      x: rect.right + 10,
      y: rect.top,
    });

    setIsLoadingSuggestion(true);
    setShowSuggestions(true);

    try {
      const suggestionResponse = await session.getFieldSuggestion(
        field.name,
        value
      );
      setSuggestion(suggestionResponse);
    } catch (error) {
      console.error("Failed to get field suggestion:", error);
      setSuggestion(null);
    } finally {
      setIsLoadingSuggestion(false);
    }
  };

  const handleFieldBlur = () => {
    // Delay hiding the popup to allow for clicks
    timeoutRef.current = window.setTimeout(() => {
      setShowSuggestions(false);
    }, 200);
  };

  const handleSuggestionSelect = (selectedSuggestion: string) => {
    onChange(field.name, selectedSuggestion);
    setShowSuggestions(false);
  };

  const handleCloseSuggestions = () => {
    setShowSuggestions(false);
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }
  };

  useEffect(() => {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>
  ) => {
    const newValue =
      field.type === "number" ? Number(e.target.value) : e.target.value;
    onChange(field.name, newValue);
  };

  const handleCheckboxChange = (optionValue: string, checked: boolean) => {
    const currentValues = Array.isArray(value) ? value : [];
    const newValues = checked
      ? [...currentValues, optionValue]
      : currentValues.filter((v: string) => v !== optionValue);
    onChange(field.name, newValues);
  };

  const renderField = () => {
    switch (field.type) {
      case "text":
        return (
          <div className="relative">
            <input
              type="text"
              id={field.name}
              value={value || ""}
              onChange={handleChange}
              onFocus={handleFieldFocus}
              onBlur={handleFieldBlur}
              placeholder={field.placeholder}
              required={field.required}
              className="w-full p-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 pr-8"
            />
            <div className="absolute right-2 top-1/2 transform -translate-y-1/2">
              <svg
                className="w-4 h-4 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"
                />
              </svg>
            </div>
          </div>
        );

      case "number":
        return (
          <div className="relative">
            <input
              type="number"
              id={field.name}
              value={value || ""}
              onChange={handleChange}
              onFocus={handleFieldFocus}
              onBlur={handleFieldBlur}
              placeholder={field.placeholder}
              required={field.required}
              min={field.min}
              max={field.max}
              className="w-full p-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 pr-8"
            />
            <div className="absolute right-2 top-1/2 transform -translate-y-1/2">
              <svg
                className="w-4 h-4 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"
                />
              </svg>
            </div>
          </div>
        );

      case "select":
        return (
          <div className="relative">
            <select
              id={field.name}
              value={value || ""}
              onChange={handleChange}
              onFocus={handleFieldFocus}
              onBlur={handleFieldBlur}
              required={field.required}
              className="w-full p-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 appearance-none pr-8"
            >
              <option value="">Select an option</option>
              {field.options?.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            <div className="absolute right-2 top-1/2 transform -translate-y-1/2 pointer-events-none">
              <svg
                className="w-4 h-4 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M19 9l-7 7-7-7"
                />
              </svg>
            </div>
          </div>
        );

      case "checkbox":
        if (field.options && field.options.length > 0) {
          return (
            <div className="space-y-2">
              {field.options.map((option) => (
                <label
                  key={option.value}
                  className="flex items-center space-x-2"
                >
                  <input
                    type="checkbox"
                    checked={
                      Array.isArray(value) && value.includes(option.value)
                    }
                    onChange={(e) =>
                      handleCheckboxChange(option.value, e.target.checked)
                    }
                    className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="text-sm">{option.label}</span>
                </label>
              ))}
            </div>
          );
        } else {
          return (
            <label className="flex items-center space-x-2">
              <input
                type="checkbox"
                id={field.name}
                checked={value || false}
                onChange={(e) => onChange(field.name, e.target.checked)}
                className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <span className="text-sm">{field.label}</span>
            </label>
          );
        }

      default:
        return null;
    }
  };

  return (
    <div ref={fieldRef} className="mb-4 relative">
      <label
        htmlFor={field.name}
        className="block text-sm font-medium text-gray-700 mb-1"
      >
        {field.label}
        {field.required && <span className="text-red-500 ml-1">*</span>}
      </label>
      {renderField()}

      <FieldSuggestionPopup
        isVisible={showSuggestions}
        position={popupPosition}
        suggestion={suggestion}
        isLoading={isLoadingSuggestion}
        onSelectSuggestion={handleSuggestionSelect}
        onClose={handleCloseSuggestions}
      />
    </div>
  );
};
