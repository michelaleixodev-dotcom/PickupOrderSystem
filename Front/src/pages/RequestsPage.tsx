import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api/client';
import { StatusBadge } from '../components/StatusBadge';
import type { PickupRequest } from '../types';

const PRIORITY_LABELS: Record<string, string> = {
  Baixa: 'Baixa',
  Normal: 'Normal',
  Alta: 'Alta',
  Urgente: 'Urgente',
};

export function RequestsPage() {
  const [requests, setRequests] = useState<PickupRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const { user } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    api
      .get<PickupRequest[]>('/pickup-requests')
      .then((r) => setRequests(r.data))
      .catch(() => setError('Erro ao carregar solicitações.'))
      .finally(() => setLoading(false));
  }, []);

  const isColaborador = user?.role === 'Colaborador';
  const canCreate = user?.role === 'Cliente' || user?.role === 'Colaborador';

  return (
    <div>
      <div className="page-header">
        <h2>Solicitações de Coleta</h2>
        {canCreate && (
          <button className="btn-primary" onClick={() => navigate('/requests/new')}>
            + Nova Solicitação
          </button>
        )}
      </div>

      {loading && <p className="text-muted">Carregando...</p>}
      {error && <p className="error-msg">{error}</p>}

      {!loading && !error && (
        <div className="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Número</th>
                {isColaborador && <th>Cliente</th>}
                <th>Remetente</th>
                <th>Destinatário</th>
                <th>Data Coleta</th>
                <th>Prioridade</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {requests.length === 0 ? (
                <tr>
                  <td colSpan={isColaborador ? 7 : 6} className="empty-row">
                    Nenhuma solicitação encontrada.
                  </td>
                </tr>
              ) : (
                requests.map((r) => (
                  <tr
                    key={r.id}
                    className="row-clickable"
                    onClick={() => navigate(`/requests/${r.id}`)}
                  >
                    <td>
                      <code>{r.identificationNumber}</code>
                    </td>
                    {isColaborador && <td>{r.clientName}</td>}
                    <td>{r.sender}</td>
                    <td>{r.recipient}</td>
                    <td>{r.scheduledPickupDate}</td>
                    <td>{PRIORITY_LABELS[r.priority] ?? r.priority}</td>
                    <td>
                      <StatusBadge status={r.status} />
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
