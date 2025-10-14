import React from "react";

export interface ApplicationData {
  id: number;
  name: string;
  description: string;
  icon: React.ReactElement;
  category: "core" | "utility";
  path?: string;
  showInNavigationBar?: boolean;
}
