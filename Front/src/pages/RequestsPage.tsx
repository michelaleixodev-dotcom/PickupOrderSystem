import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../api/client';
import { StatusBadge } from '../components/StatusBadge';
import type { PickupRequest, PagedResult } from '../types';

const PRIORITY_LABELS: Record<string, string> = {
  Baixa: 'Baixa', Normal: 'Normal', Alta: 'Alta', Urgente: 'Urgente',
};

const STATUS_OPTIONS = [
  { value: '', label: 'Todos os status' },
  { value: 'Aberta', label: 'Aberta' },
  { value: 'Atribuida', label: 'Atribuída' },
  { value: 'EmColeta', label: 'Em Coleta' },
  { value: 'Coletado', label: 'Coletado' },
  { value: 'ACaminho', label: 'A Caminho' },
  { value: 'Concluida', label: 'Concluída' },
  { value: 'FalhaNaColeta', label: 'Falha na Coleta' },
  { value: 'AguardandoDecisao', label: 'Aguardando Decisão' },
  { value: 'Cancelada', label: 'Cancelada' },
];

const TERMINAL_STATUSES = new Set(['Concluida', 'Cancelada']);

function isOverdue(r: PickupRequest): boolean {
  if (TERMINAL_STATUSES.has(r.status)) return false;
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const scheduled = new Date(r.scheduledPickupDate + 'T00:00:00');
  return scheduled < today;
}

export function RequestsPage() {
  const [paged, setPaged] = useState<PagedResult<PickupRequest> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const { user } = useAuth();
  const navigate = useNavigate();

  const [statusFilter, setStatusFilter] = useState('');
  const [clientFilter, setClientFilter] = useState('');
  const [fromFilter, setFromFilter] = useState('');
  const [toFilter, setToFilter] = useState('');
  const [page, setPage] = useState(1);
  const PAGE_SIZE = 10;

  const isColaborador = user?.role === 'Colaborador';
  const canCreate = user?.role === 'Cliente' || user?.role === 'Colaborador';

  useEffect(() => {
    setLoading(true);
    const params = new URLSearchParams();
    if (statusFilter) params.set('status', statusFilter);
    if (isColaborador && clientFilter) params.set('clientName', clientFilter);
    if (fromFilter) params.set('from', fromFilter);
    if (toFilter) params.set('to', toFilter);
    params.set('page', String(page));
    params.set('pageSize', String(PAGE_SIZE));

    api
      .get<PagedResult<PickupRequest>>(`/pickup-requests?${params.toString()}`)
      .then((r) => setPaged(r.data))
      .catch(() => setError('Erro ao carregar solicitações.'))
      .finally(() => setLoading(false));
  }, [statusFilter, clientFilter, fromFilter, toFilter, page, isColaborador]);

  function clearFilters() {
    setStatusFilter('');
    setClientFilter('');
    setFromFilter('');
    setToFilter('');
    setPage(1);
  }

  const hasFilters = statusFilter || clientFilter || fromFilter || toFilter;

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

      <div className="filter-bar">
        <div className="form-group">
          <label>Status</label>
          <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}>
            {STATUS_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>

        {isColaborador && (
          <div className="form-group">
            <label>Cliente</label>
            <input
              type="text"
              placeholder="Buscar por nome..."
              value={clientFilter}
              onChange={(e) => { setClientFilter(e.target.value); setPage(1); }}
            />
          </div>
        )}

        <div className="form-group">
          <label>De</label>
          <input type="date" value={fromFilter} onChange={(e) => { setFromFilter(e.target.value); setPage(1); }} />
        </div>

        <div className="form-group">
          <label>Até</label>
          <input type="date" value={toFilter} onChange={(e) => { setToFilter(e.target.value); setPage(1); }} />
        </div>

        {hasFilters && (
          <div className="form-group" style={{ justifyContent: 'flex-end' }}>
            <label style={{ visibility: 'hidden' }}>x</label>
            <button className="btn-secondary" onClick={clearFilters}>Limpar filtros</button>
          </div>
        )}
      </div>

      {loading && <p className="text-muted">Carregando...</p>}
      {error && <p className="error-msg">{error}</p>}

      {!loading && !error && (
        <>
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
                {(paged?.items ?? []).length === 0 ? (
                  <tr>
                    <td colSpan={isColaborador ? 7 : 6} className="empty-row">
                      Nenhuma solicitação encontrada.
                    </td>
                  </tr>
                ) : (
                  (paged?.items ?? []).map((r) => {
                    const overdue = isOverdue(r);
                    const rowClass = [
                      'row-clickable',
                      r.priority === 'Urgente' ? 'row-urgente' : '',
                      r.priority === 'Alta' ? 'row-alta' : '',
                    ].filter(Boolean).join(' ');

                    return (
                      <tr key={r.id} className={rowClass} onClick={() => navigate(`/requests/${r.id}`)}>
                        <td><code>{r.identificationNumber}</code></td>
                        {isColaborador && <td>{r.clientName}</td>}
                        <td>{r.sender}</td>
                        <td>{r.recipient}</td>
                        <td>
                          {r.scheduledPickupDate}
                          {overdue && <span className="overdue-badge">Em Atraso</span>}
                        </td>
                        <td>
                          <span className={`priority-badge priority-${r.priority.toLowerCase()}`}>
                            {PRIORITY_LABELS[r.priority] ?? r.priority}
                          </span>
                        </td>
                        <td><StatusBadge status={r.status} /></td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>

          {paged && paged.totalPages > 1 && (
            <div className="pagination">
              <button
                className="btn-secondary"
                onClick={() => setPage((p) => p - 1)}
                disabled={page <= 1}
              >
                ← Anterior
              </button>
              <span className="pagination-info">
                Página {paged.page} de {paged.totalPages}
                <span className="pagination-total"> ({paged.totalItems} registros)</span>
              </span>
              <button
                className="btn-secondary"
                onClick={() => setPage((p) => p + 1)}
                disabled={page >= paged.totalPages}
              >
                Próxima →
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
