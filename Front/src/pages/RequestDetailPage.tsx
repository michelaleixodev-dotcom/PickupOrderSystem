import { useEffect, useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api/client';
import { StatusBadge } from '../components/StatusBadge';
import type { PickupRequest } from '../types';

const ALL_STATUSES = [
  { value: 'Aberta', label: 'Aberta' },
  { value: 'Atribuida', label: 'Atribuída' },
  { value: 'EmAndamento', label: 'Em Andamento' },
  { value: 'Concluida', label: 'Concluída' },
  { value: 'FalhaNaColeta', label: 'Falha na Coleta' },
  { value: 'Cancelada', label: 'Cancelada' },
];

const PRIORITY_LABELS: Record<string, string> = {
  Baixa: 'Baixa',
  Normal: 'Normal',
  Alta: 'Alta',
  Urgente: 'Urgente',
};

export function RequestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [request, setRequest] = useState<PickupRequest | null>(null);
  const [newStatus, setNewStatus] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    api
      .get<PickupRequest>(`/pickup-requests/${id}`)
      .then((r) => {
        setRequest(r.data);
        setNewStatus(r.data.status);
      })
      .catch(() => setError('Solicitação não encontrada.'))
      .finally(() => setLoading(false));
  }, [id]);

  async function handleStatusUpdate() {
    if (!request || newStatus === request.status) return;
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      await api.patch(`/pickup-requests/${id}/status`, { status: newStatus });
      setRequest((prev) => (prev ? { ...prev, status: newStatus } : prev));
      setSuccess('Status atualizado com sucesso!');
    } catch {
      setError('Erro ao atualizar status.');
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <p className="text-muted">Carregando...</p>;
  if (!request) return <p className="error-msg">{error || 'Não encontrado.'}</p>;

  return (
    <div>
      <div className="page-header">
        <h2>
          Solicitação <code>{request.identificationNumber}</code>
        </h2>
        <button className="btn-secondary" onClick={() => navigate('/requests')}>
          Voltar
        </button>
      </div>

      <div className="card">
        <div className="detail-grid">
          <DetailRow label="Status" value={<StatusBadge status={request.status} />} />
          <DetailRow label="Prioridade" value={PRIORITY_LABELS[request.priority] ?? request.priority} />
          <DetailRow label="Cliente" value={request.clientName} />
          <DetailRow label="Remetente" value={request.sender} />
          <DetailRow label="Endereço de Coleta" value={request.pickupAddress} />
          <DetailRow label="Destinatário" value={request.recipient} />
          <DetailRow label="Endereço de Entrega" value={request.deliveryAddress} />
          <DetailRow label="Data de Coleta Prevista" value={request.scheduledPickupDate} />
          <DetailRow
            label="Data da Solicitação"
            value={new Date(request.requestDate).toLocaleDateString('pt-BR')}
          />
          {request.notes && <DetailRow label="Observações" value={request.notes} />}
        </div>
      </div>

      {user?.role === 'Colaborador' && (
        <div className="card" style={{ marginTop: 16 }}>
          <h3>Atualizar Status</h3>
          <div className="status-update-row">
            <select value={newStatus} onChange={(e) => setNewStatus(e.target.value)}>
              {ALL_STATUSES.map((s) => (
                <option key={s.value} value={s.value}>
                  {s.label}
                </option>
              ))}
            </select>
            <button
              className="btn-primary"
              onClick={handleStatusUpdate}
              disabled={saving || newStatus === request.status}
            >
              {saving ? 'Salvando...' : 'Salvar'}
            </button>
          </div>
          {error && <p className="error-msg">{error}</p>}
          {success && <p className="success-msg">{success}</p>}
        </div>
      )}
    </div>
  );
}

function DetailRow({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="detail-row">
      <span className="detail-label">{label}</span>
      <span className="detail-value">{value}</span>
    </div>
  );
}
