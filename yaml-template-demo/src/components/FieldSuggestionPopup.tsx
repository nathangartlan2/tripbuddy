import React, { useState, useEffect } from "react";
import { FieldSuggestionResponse } from "../services/TripBuddyAPI";

interface FieldSuggestionPopupProps {
  isVisible: boolean;
  position: { x: number; y: number };
  suggestion: FieldSuggestionResponse | null;
  isLoading: boolean;
  onSelectSuggestion: (suggestion: string) => void;
  onClose: () => void;
}

export const FieldSuggestionPopup: React.FC<FieldSuggestionPopupProps> = ({
  isVisible,
  position,
  suggestion,
  isLoading,
  onSelectSuggestion,
  onClose,
}) => {
  const [adjustedPosition, setAdjustedPosition] = useState(position);

  useEffect(() => {
    // Adjust position to prevent popup from going off-screen
    const adjustPosition = () => {
      const popup = document.getElementById("field-suggestion-popup");
      if (!popup) return;

      const rect = popup.getBoundingClientRect();
      const viewportWidth = window.innerWidth;
      const viewportHeight = window.innerHeight;

      let adjustedX = position.x;
      let adjustedY = position.y;

      // Adjust horizontal position
      if (position.x + rect.width > viewportWidth) {
        adjustedX = viewportWidth - rect.width - 10;
      }

      // Adjust vertical position
      if (position.y + rect.height > viewportHeight) {
        adjustedY = position.y - rect.height - 10;
      }

      setAdjustedPosition({ x: adjustedX, y: adjustedY });
    };

    if (isVisible) {
      // Use a timeout to allow the popup to render first
      setTimeout(adjustPosition, 10);
    }
  }, [isVisible, position]);

  if (!isVisible) return null;

  return (
    <div
      id="field-suggestion-popup"
      className="fixed z-50 bg-white rounded-lg shadow-lg border border-gray-200 p-4 max-w-sm"
      style={{
        left: `${adjustedPosition.x}px`,
        top: `${adjustedPosition.y}px`,
      }}
    >
      {/* Close button */}
      <button
        onClick={onClose}
        className="absolute top-2 right-2 text-gray-400 hover:text-gray-600"
      >
        <svg
          className="w-4 h-4"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M6 18L18 6M6 6l12 12"
          />
        </svg>
      </button>

      {isLoading ? (
        <div className="flex items-center space-x-2">
          <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
          <span className="text-sm text-gray-600">Getting suggestions...</span>
        </div>
      ) : suggestion ? (
        <div>
          <div className="flex items-center space-x-2 mb-3">
            <svg
              className="w-4 h-4 text-blue-600"
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
            <h3 className="font-medium text-gray-800">AI Suggestions</h3>
          </div>

          {suggestion.suggestions.length > 0 ? (
            <div className="space-y-2 mb-3">
              {suggestion.suggestions.map((item, index) => (
                <button
                  key={index}
                  onClick={() => onSelectSuggestion(item)}
                  className="w-full text-left p-2 rounded bg-gray-50 hover:bg-blue-50 text-sm text-gray-700 hover:text-blue-700 transition-colors"
                >
                  {item}
                </button>
              ))}
            </div>
          ) : (
            <p className="text-sm text-gray-500 mb-3">
              No suggestions available for this field.
            </p>
          )}

          {suggestion.explanation && (
            <div className="border-t pt-2">
              <p className="text-xs text-gray-600">{suggestion.explanation}</p>
            </div>
          )}
        </div>
      ) : (
        <p className="text-sm text-gray-500">Unable to load suggestions.</p>
      )}
    </div>
  );
};
