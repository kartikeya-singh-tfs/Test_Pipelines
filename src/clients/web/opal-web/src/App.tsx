import React from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from "react-router-dom";
import Login from "./platform/auth/Login";
import { FavoritedAppsProvider } from "./shared/components/MainLayout";
import {
  GlobalSSEProvider,
  UserProvider,
  NotificationProvider,
} from "./shared";
import { AppRouter } from "./shell/AppRouter";

const App: React.FC = () => {
  return (
    <Router>
      <UserProvider>
        <GlobalSSEProvider>
          <NotificationProvider>
            <FavoritedAppsProvider>
              <Routes>
                <Route path="/login" element={<Login />} />
                <Route path="/*" element={<AppRouter />} />
                <Route path="/" element={<Navigate to="/login" replace />} />
              </Routes>
            </FavoritedAppsProvider>
          </NotificationProvider>
        </GlobalSSEProvider>
      </UserProvider>
    </Router>
  );
};

export default App;
