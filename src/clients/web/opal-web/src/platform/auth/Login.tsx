import React, { useState } from "react";
import {
  TextField,
  Button,
  Card,
  CardContent,
  Box,
  Typography,
} from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useUser } from "../../shared/contexts/UserContext";

const Login: React.FC = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const navigate = useNavigate();
  const { setCurrentUser } = useUser();

  const isLoginEnabled = username.trim() !== "" && password.trim() !== "";

  const handleCancel = () => {
    setUsername("");
    setPassword("");
  };

  const handleSignIn = () => {
    // For demo: set user with entered username and a generated email
    setCurrentUser({
      id: "1",
      username: username,
      email: `${username}@thermofisher.com`,
      firstName: "",
      lastName: "",
      avatar: "",
      role: "",
      department: "",
    });
    navigate("/home");
  };

  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        height: "100vh",
        backgroundColor: "#f5f5f5",
        flexDirection: "column",
        overflow: "hidden",
      }}
    >
      {/* Logo Section */}
      <Box sx={{ textAlign: "center", mb: 3 }}>
        <Typography
          sx={{
            fontSize: { xs: "28px", sm: "32px" },
            fontWeight: 500,
            lineHeight: 1,
            mb: 0.5,
            fontFamily:
              '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
          }}
        >
          <Box component="span" sx={{ color: "#e71316" }}>
            thermo
          </Box>
          <Box
            component="span"
            sx={{ color: "#6b6b6b", fontWeight: 400, ml: 1 }}
          >
            scientific
          </Box>
        </Typography>
        <Typography
          sx={{
            fontSize: { xs: "20px", sm: "24px" },
            color: "#6b6b6b",
            fontWeight: 400,
            fontFamily:
              '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
          }}
        >
          OPAL
        </Typography>
      </Box>

      {/* Login Card */}
      <Card
        sx={{
          width: { xs: "100%", sm: 420 },
          maxWidth: 420,
          boxShadow: "0px 12px 32px rgba(0, 0, 0, 0.12)",
          borderRadius: 3,
          border: "1px solid rgba(0, 0, 0, 0.08)",
        }}
      >
        <CardContent sx={{ p: { xs: 3, sm: 4 } }}>
          {/* Welcome Header */}
          <Box sx={{ textAlign: "center", mb: 3 }}>
            <Typography
              variant="h4"
              sx={{
                fontWeight: 600,
                color: "#1a1a1a",
                mb: 1,
                fontSize: { xs: "24px", sm: "28px" },
              }}
            >
              Welcome
            </Typography>
            <Typography
              variant="body1"
              sx={{
                color: "#666",
                fontSize: "16px",
              }}
            >
              Sign in to your account
            </Typography>
          </Box>

          {/* Login Form */}
          <Box
            component="form"
            sx={{ display: "flex", flexDirection: "column", gap: 2 }}
          >
            <TextField
              label="Username"
              variant="outlined"
              fullWidth
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              sx={{
                "& .MuiOutlinedInput-root": {
                  borderRadius: 2,
                },
              }}
            />
            <TextField
              label="Password"
              type="password"
              variant="outlined"
              fullWidth
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              sx={{
                "& .MuiOutlinedInput-root": {
                  borderRadius: 2,
                },
              }}
            />

            {/* Button Group */}
            <Box sx={{ display: "flex", gap: 1.5, mt: 0.5 }}>
              <Button
                variant="outlined"
                fullWidth
                sx={{
                  py: 1.5,
                  textTransform: "none",
                  fontSize: "16px",
                  borderRadius: 2,
                  fontWeight: 500,
                }}
                onClick={handleCancel}
              >
                Cancel
              </Button>
              <Button
                variant="contained"
                fullWidth
                sx={{
                  py: 1.5,
                  textTransform: "none",
                  fontSize: "16px",
                  borderRadius: 2,
                  fontWeight: 500,
                  boxShadow: "0 4px 12px rgba(25, 118, 210, 0.25)",
                  "&:hover": {
                    boxShadow: "0 6px 16px rgba(25, 118, 210, 0.35)",
                  },
                }}
                disabled={!isLoginEnabled}
                onClick={handleSignIn}
              >
                Sign In
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};

export default Login;
