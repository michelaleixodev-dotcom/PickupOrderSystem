import { useEffect, useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api/client';
import { StatusBadge } from '../components/StatusBadge';
import type { PickupRequest, DriverOption, VehicleOption } from '../types';

interface AdvanceAction {
  label: string;
  status: string;
  variant?: 'primary' | 'warning';
  modal?: 'fail';
}

interface StatusAction {
  question: string;
  advance: AdvanceAction[];
}

const STATUS_ACTIONS: Record<string, StatusAction> = {
  Atribuida: {
    question: 'O motorista chegou ao local de coleta?',
    advance: [{ label: 'Confirmar chegada ao local', status: 'EmColeta' }],
  },
  EmColeta: {
    question: 'A coleta foi realizada?',
    advance: [
      { label: 'Confirmar coleta realizada', status: 'Coletado' },
      { label: 'Registrar falha na coleta', status: 'FalhaNaColeta', variant: 'warning', modal: 'fail' },
    ],
  },
  Coletado: {
    question: 'O motorista está a caminho da entrega?',
    advance: [{ label: 'Confirmar saída para entrega', status: 'ACaminho' }],
  },
  ACaminho: {
    question: 'A entrega foi realizada?',
    advance: [{ label: 'Confirmar entrega realizada', status: 'Concluida' }],
  },
  FalhaNaColeta: {
    question: 'Deseja encaminhar para análise logística?',
    advance: [{ label: 'Encaminhar para decisão logística', status: 'AguardandoDecisao' }],
  },
  AguardandoDecisao: {
    question: 'Atribua um motorista para retomar o pedido.',
    advance: [],
  },
};

const CANCELABLE = new Set(['Aberta', 'Atribuida', 'EmColeta', 'Coletado', 'ACaminho', 'FalhaNaColeta', 'AguardandoDecisao']);

const STATUS_LABELS: Record<string, string> = {
  Aberta: 'Aberta', Atribuida: 'Atribuída', EmColeta: 'Em Coleta',
  Coletado: 'Coletado', ACaminho: 'A Caminho', Concluida: 'Concluída', FalhaNaColeta: 'Falha na Coleta',
  AguardandoDecisao: 'Aguardando Decisão', Cancelada: 'Cancelada',
};

const OCCURRENCE_LABELS: Record<string, string> = {
  RemetenteIndisponivel: 'Remetente Indisponível',
  EnderecoIncorreto: 'Endereço Incorreto',
  CargaDivergente: 'Carga Divergente',
  VeiculoAvaria: 'Veículo com Avaria',
  AcessoNegado: 'Acesso Negado',
  Outro: 'Outro',
};

const PRIORITY_LABELS: Record<string, string> = {
  Baixa: 'Baixa', Normal: 'Normal', Alta: 'Alta', Urgente: 'Urgente',
};

export function RequestDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [request, setRequest] = useState<PickupRequest | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [drivers, setDrivers] = useState<DriverOption[]>([]);
  const [vehicles, setVehicles] = useState<VehicleOption[]>([]);
  const [driverId, setDriverId] = useState('');
  const [vehicleId, setVehicleId] = useState('');
  const [assigning, setAssigning] = useState(false);
  const [assignError, setAssignError] = useState('');

  const [showFailModal, setShowFailModal] = useState(false);
  const [failType, setFailType] = useState('RemetenteIndisponivel');
  const [failDesc, setFailDesc] = useState('');
  const [savingFail, setSavingFail] = useState(false);
  const [failError, setFailError] = useState('');

  const isColaborador = user?.role === 'Colaborador';

  useEffect(() => {
    api
      .get<PickupRequest>(`/pickup-requests/${id}`)
      .then((r) => setRequest(r.data))
      .catch(() => setError('Solicitação não encontrada.'))
      .finally(() => setLoading(false));
  }, [id]);

  useEffect(() => {
    const needsAssign = request?.status === 'Aberta' || request?.status === 'AguardandoDecisao';
    if (!isColaborador || !needsAssign) return;
    Promise.all([api.get<DriverOption[]>('/drivers'), api.get<VehicleOption[]>('/vehicles')]).then(([d, v]) => {
      setDrivers(d.data);
      setVehicles(v.data);
      if (d.data.length > 0) setDriverId(d.data[0].id);
      if (v.data.length > 0) setVehicleId(v.data[0].id);
    });
  }, [isColaborador, request?.status]);

  async function advanceTo(status: string) {
    setSaving(true);
    setError('');
    setSuccess('');
    try {
      await api.patch(`/pickup-requests/${id}/status`, { status });
      const updated = await api.get<PickupRequest>(`/pickup-requests/${id}`);
      setRequest(updated.data);
      setSuccess('Status atualizado.');
    } catch {
      setError('Erro ao atualizar status.');
    } finally {
      setSaving(false);
    }
  }

  async function handleAssign() {
    if (!driverId || !vehicleId) return;
    setAssigning(true);
    setAssignError('');
    try {
      await api.post(`/pickup-requests/${id}/assign`, { driverId, vehicleId });
      const updated = await api.get<PickupRequest>(`/pickup-requests/${id}`);
      setRequest(updated.data);
    } catch {
      setAssignError('Erro ao atribuir. Tente novamente.');
    } finally {
      setAssigning(false);
    }
  }

  async function handleFail() {
    if (!failDesc.trim()) return;
    setSavingFail(true);
    setFailError('');
    try {
      await api.post(`/pickup-requests/${id}/fail`, { type: failType, description: failDesc });
      const updated = await api.get<PickupRequest>(`/pickup-requests/${id}`);
      setRequest(updated.data);
      setShowFailModal(false);
      setFailDesc('');
    } catch {
      setFailError('Erro ao registrar falha.');
    } finally {
      setSavingFail(false);
    }
  }

  if (loading) return <p className="text-muted">Carregando...</p>;
  if (!request) return <p className="error-msg">{error || 'Não encontrado.'}</p>;

  const action = STATUS_ACTIONS[request.status];

  return (
    <div>
      <div className="page-header">
        <h2>Solicitação <code>{request.identificationNumber}</code></h2>
        <button className="btn-secondary" onClick={() => navigate('/requests')}>Voltar</button>
      </div>

      {isColaborador && (request.status === 'Aberta' || request.status === 'AguardandoDecisao') && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginBottom: 12, fontSize: 15, fontWeight: 600 }}>
            {request.status === 'AguardandoDecisao' ? 'Reatribuir Motorista e Veículo' : 'Atribuir Motorista e Veículo'}
          </h3>
          <div className="form-grid">
            <div className="form-group">
              <label>Motorista</label>
              <select value={driverId} onChange={(e) => setDriverId(e.target.value)}>
                {drivers.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
              </select>
            </div>
            <div className="form-group">
              <label>Veículo</label>
              <select value={vehicleId} onChange={(e) => setVehicleId(e.target.value)}>
                {vehicles.map((v) => <option key={v.id} value={v.id}>{v.model} — {v.licensePlate}</option>)}
              </select>
            </div>
          </div>
          <div style={{ marginTop: 12, display: 'flex', gap: 12, alignItems: 'center' }}>
            <button className="btn-primary" onClick={handleAssign} disabled={assigning || !driverId || !vehicleId}>
              {assigning ? 'Atribuindo...' : 'Confirmar Atribuição'}
            </button>
            {assignError && <span className="cep-hint cep-error">{assignError}</span>}
          </div>
        </div>
      )}

      {isColaborador && (action || CANCELABLE.has(request.status)) && (
        <div className="card action-card" style={{ marginBottom: 16 }}>
          {action && (
            <>
              <p className="action-question">{action.question}</p>
              <div className="action-buttons">
                {action.advance.map((a) => (
                  <button
                    key={a.status}
                    className={a.variant === 'warning' ? 'btn-warning' : 'btn-primary'}
                    onClick={() => a.modal === 'fail' ? setShowFailModal(true) : advanceTo(a.status)}
                    disabled={saving}
                  >
                    {a.label}
                  </button>
                ))}
              </div>
            </>
          )}
          {CANCELABLE.has(request.status) && (
            <div className={action ? 'cancel-row' : ''}>
              <button className="btn-danger" onClick={() => advanceTo('Cancelada')} disabled={saving}>
                Cancelar pedido
              </button>
            </div>
          )}
          {error && <p className="error-msg" style={{ marginTop: 8 }}>{error}</p>}
          {success && <p className="success-msg">{success}</p>}
        </div>
      )}

      <div className="detail-layout">
        <div className="detail-main">
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
              <DetailRow label="Data da Solicitação" value={new Date(request.requestDate).toLocaleDateString('pt-BR')} />
              {request.notes && <DetailRow label="Observações" value={request.notes} />}
            </div>
          </div>

          {(request.statusHistory?.length ?? 0) > 0 && (
            <div className="card" style={{ marginTop: 16 }}>
              <h3 style={{ marginBottom: 14, fontSize: 15, fontWeight: 600 }}>Histórico de Status</h3>
              <div className="timeline">
                {request.statusHistory?.map((h, i) => (
                  <div key={i} className="timeline-item">
                    <div className="timeline-dot" />
                    <div className="timeline-content">
                      <span className="timeline-label">
                        {h.fromStatus ? `${STATUS_LABELS[h.fromStatus] ?? h.fromStatus} → ` : ''}
                        <strong>{STATUS_LABELS[h.toStatus] ?? h.toStatus}</strong>
                      </span>
                      <span className="timeline-meta">
                        {new Date(h.changedAt).toLocaleString('pt-BR')} · {h.changedBy}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {request.assignment && (
            <div className="card" style={{ marginTop: 16 }}>
              <h3 style={{ marginBottom: 12, fontSize: 15, fontWeight: 600 }}>Atribuição</h3>
              <div className="detail-grid">
                <DetailRow label="Motorista" value={request.assignment.driverName} />
                <DetailRow label="Veículo" value={`${request.assignment.vehicleModel} — ${request.assignment.vehiclePlate}`} />
                <DetailRow label="Data de Atribuição" value={new Date(request.assignment.assignmentDate).toLocaleString('pt-BR')} />
              </div>
            </div>
          )}
        </div>

        <div className="detail-aside">
          <div className="card">
            <h3 style={{ marginBottom: 14, fontSize: 15, fontWeight: 600 }}>Ocorrências</h3>
            {(request.occurrences?.length ?? 0) === 0 ? (
              <p className="text-muted" style={{ fontSize: 13 }}>Nenhuma ocorrência registrada.</p>
            ) : (
              request.occurrences?.map((o) => (
                <div key={o.id} className="occurrence-item">
                  <div className="occurrence-header">
                    <span className="occurrence-type-badge">{OCCURRENCE_LABELS[o.type] ?? o.type}</span>
                    {o.resolved && <span className="occurrence-resolved">Resolvida</span>}
                  </div>
                  <span className="occurrence-meta" style={{ marginLeft: 0, display: 'block', marginBottom: 4 }}>
                    {new Date(o.occurrenceDate).toLocaleString('pt-BR')} · {o.registeredBy}
                  </span>
                  <p className="occurrence-desc">{o.description}</p>
                  {o.resolutionNotes && (
                    <p className="occurrence-resolution">Resolução: {o.resolutionNotes}</p>
                  )}
                </div>
              ))
            )}
          </div>
        </div>
      </div>

      {showFailModal && (
        <div className="modal-overlay" onClick={() => setShowFailModal(false)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <h3 className="modal-title">Registrar Falha na Coleta</h3>
            <p className="modal-subtitle">Descreva o que impediu a coleta. Uma ocorrência será registrada automaticamente.</p>
            <div className="form-group" style={{ marginBottom: 12 }}>
              <label>Tipo</label>
              <select value={failType} onChange={(e) => setFailType(e.target.value)}>
                {Object.entries(OCCURRENCE_LABELS).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
            </div>
            <div className="form-group" style={{ marginBottom: 16 }}>
              <label>Descrição</label>
              <textarea
                value={failDesc}
                onChange={(e) => setFailDesc(e.target.value)}
                rows={3}
                placeholder="Descreva o que ocorreu..."
                autoFocus
              />
            </div>
            {failError && <p className="error-msg" style={{ marginBottom: 12 }}>{failError}</p>}
            <div className="modal-actions">
              <button className="btn-secondary" onClick={() => setShowFailModal(false)} disabled={savingFail}>
                Cancelar
              </button>
              <button className="btn-warning" onClick={handleFail} disabled={savingFail || !failDesc.trim()}>
                {savingFail ? 'Registrando...' : 'Confirmar Falha'}
              </button>
            </div>
          </div>
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
