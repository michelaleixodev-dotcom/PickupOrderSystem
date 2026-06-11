import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { PrivateRoute } from './components/PrivateRoute';
import { Layout } from './components/Layout';
import { LoginPage } from './pages/LoginPage';
import { RequestsPage } from './pages/RequestsPage';
import { NewRequestPage } from './pages/NewRequestPage';
import { RequestDetailPage } from './pages/RequestDetailPage';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/requests"
            element={
              <PrivateRoute>
                <Layout>
                  <RequestsPage />
                </Layout>
              </PrivateRoute>
            }
          />
          <Route
            path="/requests/new"
            element={
              <PrivateRoute>
                <Layout>
                  <NewRequestPage />
                </Layout>
              </PrivateRoute>
            }
          />
          <Route
            path="/requests/:id"
            element={
              <PrivateRoute>
                <Layout>
                  <RequestDetailPage />
                </Layout>
              </PrivateRoute>
            }
          />
          <Route path="*" element={<Navigate to="/requests" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
