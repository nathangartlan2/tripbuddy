import { useState, useEffect } from "react";
import { Template, FormData } from "./types";
import { FormRenderer } from "./components/FormRenderer";
import { TripPlanningSession } from "./services/TripBuddyAPI";
import "./index.css";

function App() {
  const [session] = useState(new TripPlanningSession());
  const [template, setTemplate] = useState<Template | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const initializeSession = async () => {
      try {
        const response = await session.initialize("backpacking");
        setTemplate(response.template);
        setLoading(false);
      } catch (err) {
        setError(
          `Failed to initialize session: ${
            err instanceof Error ? err.message : "Unknown error"
          }`
        );
        setLoading(false);
      }
    };

    initializeSession();
  }, [session]);

  const handleFormSubmit = (formData: FormData) => {
    console.log("Form submitted with data:", formData);
    alert(
      `Trip plan generated! Check console for full data.\n\nTrip Type: ${
        formData.trip_type || "Not specified"
      }\nDestination: ${formData.destination || "Not specified"}`
    );
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading template...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 max-w-md">
          <h2 className="text-red-800 font-semibold mb-2">
            Error Loading Template
          </h2>
          <p className="text-red-600">{error}</p>
        </div>
      </div>
    );
  }

  if (!template) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-600">No template found</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <FormRenderer template={template} onSubmit={handleFormSubmit} />
    </div>
  );
}

export default App;
