import React, { createContext, useContext, useState, ReactNode } from "react";

export interface User {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  avatar?: string;
  role: string;
  department: string;
}

interface UserContextType {
  currentUser: User | null;
  setCurrentUser: (user: User | null) => void;
  logout: () => void;
}

const UserContext = createContext<UserContextType | undefined>(undefined);

export const useUser = () => {
  const context = useContext(UserContext);
  if (context === undefined) {
    throw new Error("useUser must be used within a UserProvider");
  }
  return context;
};

interface UserProviderProps {
  children: ReactNode;
}

export const UserProvider: React.FC<UserProviderProps> = ({ children }) => {
  // Try to load user from localStorage on first render
  const [currentUser, setCurrentUserState] = useState<User | null>(() => {
    const stored = localStorage.getItem("opal_current_user");
    return stored ? JSON.parse(stored) : null;
  });

  // Wrap setCurrentUser to persist to localStorage
  const setCurrentUser = (user: User | null) => {
    setCurrentUserState(user);
    if (user) {
      localStorage.setItem("opal_current_user", JSON.stringify(user));
    } else {
      localStorage.removeItem("opal_current_user");
    }
  };

  const logout = () => {
    setCurrentUser(null);
    // Additional logout logic can be added here
  };

  const value = {
    currentUser,
    setCurrentUser,
    logout,
  };

  return <UserContext.Provider value={value}>{children}</UserContext.Provider>;
};

// Mock user data for demonstration
export const mockUser: User = {
  id: "1",
  username: "jsmith",
  email: "john.smith@company.com",
  firstName: "John",
  lastName: "Smith",
  // avatar: '/static/images/avatar/user.jpg', // Using initials instead
  role: "Senior Analyst",
  department: "Research & Development",
};
