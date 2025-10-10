import { useState, useEffect } from "react";
import { Template, FormData } from "./types";
import { FormRenderer } from "./components/FormRendererNew";
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

    // Create a summary
    const summary = [];
    if (formData.destination)
      summary.push(`Destination: ${formData.destination}`);
    if (formData.duration) summary.push(`Duration: ${formData.duration} days`);
    if (formData.group_size) summary.push(`Group size: ${formData.group_size}`);
    if (formData.season) summary.push(`Season: ${formData.season}`);

    alert(
      `🏕️ Trip Plan Generated!\n\n${summary.join(
        "\n"
      )}\n\nFull details are logged to console.`
    );
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Initializing AI session...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 max-w-md">
          <h2 className="text-red-800 font-semibold mb-2">Connection Error</h2>
          <p className="text-red-600 mb-4">{error}</p>
          <p className="text-sm text-red-500">
            Make sure the TripBuddy API is running on https://localhost:5001
          </p>
        </div>
      </div>
    );
  }

  if (!template) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-600">No template loaded</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <FormRenderer
        template={template}
        session={session}
        onSubmit={handleFormSubmit}
      />
    </div>
  );
}

export default App;
