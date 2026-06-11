import { useState, type FormEvent, type ChangeEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/client';
import type { CreatePickupRequest } from '../types';

interface CepState {
  loading: boolean;
  error: string;
}

const emptyCep: CepState = { loading: false, error: '' };

export function NewRequestPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<CreatePickupRequest>({
    sender: '',
    pickupAddress: '',
    recipient: '',
    deliveryAddress: '',
    scheduledPickupDate: '',
    priority: 'Normal',
    notes: '',
  });
  const [pickupCep, setPickupCep] = useState('');
  const [deliveryCep, setDeliveryCep] = useState('');
  const [pickupCepState, setPickupCepState] = useState<CepState>(emptyCep);
  const [deliveryCepState, setDeliveryCepState] = useState<CepState>(emptyCep);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  function handleChange(e: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  }

  function formatCep(value: string) {
    const digits = value.replace(/\D/g, '').slice(0, 8);
    return digits.length > 5 ? `${digits.slice(0, 5)}-${digits.slice(5)}` : digits;
  }

  async function lookupCep(digits: string, field: 'pickupAddress' | 'deliveryAddress') {
    const setState = field === 'pickupAddress' ? setPickupCepState : setDeliveryCepState;
    setState({ loading: true, error: '' });
    try {
      const res = await fetch(`https://viacep.com.br/ws/${digits}/json/`);
      const data = await res.json();
      if (data.erro) {
        setState({ loading: false, error: 'CEP não encontrado.' });
        return;
      }
      const address = [data.logradouro, data.bairro, `${data.localidade}/${data.uf}`]
        .filter(Boolean)
        .join(', ');
      setForm((prev) => ({ ...prev, [field]: address }));
      setState({ loading: false, error: '' });
    } catch {
      setState({ loading: false, error: 'Erro ao consultar o CEP.' });
    }
  }

  function handleCepChange(
    e: ChangeEvent<HTMLInputElement>,
    setter: (v: string) => void,
    field: 'pickupAddress' | 'deliveryAddress',
  ) {
    const formatted = formatCep(e.target.value);
    setter(formatted);
    const digits = formatted.replace(/\D/g, '');
    if (digits.length === 8) lookupCep(digits, field);
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await api.post('/pickup-requests', form);
      navigate('/requests');
    } catch {
      setError('Erro ao criar solicitação. Verifique os dados e tente novamente.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <div className="page-header">
        <h2>Nova Solicitação de Coleta</h2>
        <button className="btn-secondary" onClick={() => navigate('/requests')}>
          Voltar
        </button>
      </div>
      <div className="card">
        <form onSubmit={handleSubmit} className="form-grid">

          <div className="form-group">
            <label>Remetente</label>
            <input name="sender" value={form.sender} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Destinatário</label>
            <input name="recipient" value={form.recipient} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>CEP de Coleta</label>
            <input
              placeholder="00000-000"
              value={pickupCep}
              onChange={(e) => handleCepChange(e, setPickupCep, 'pickupAddress')}
              maxLength={9}
            />
            {pickupCepState.loading && <span className="cep-hint">Buscando...</span>}
            {pickupCepState.error && <span className="cep-hint cep-error">{pickupCepState.error}</span>}
          </div>
          <div className="form-group">
            <label>CEP de Entrega</label>
            <input
              placeholder="00000-000"
              value={deliveryCep}
              onChange={(e) => handleCepChange(e, setDeliveryCep, 'deliveryAddress')}
              maxLength={9}
            />
            {deliveryCepState.loading && <span className="cep-hint">Buscando...</span>}
            {deliveryCepState.error && <span className="cep-hint cep-error">{deliveryCepState.error}</span>}
          </div>

          <div className="form-group">
            <label>Endereço de Coleta</label>
            <input name="pickupAddress" value={form.pickupAddress} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Endereço de Entrega</label>
            <input name="deliveryAddress" value={form.deliveryAddress} onChange={handleChange} required />
          </div>

          <div className="form-group">
            <label>Data de Coleta</label>
            <input
              type="date"
              name="scheduledPickupDate"
              value={form.scheduledPickupDate}
              onChange={handleChange}
              required
            />
          </div>
          <div className="form-group">
            <label>Prioridade</label>
            <select name="priority" value={form.priority} onChange={handleChange}>
              <option value="Baixa">Baixa</option>
              <option value="Normal">Normal</option>
              <option value="Alta">Alta</option>
              <option value="Urgente">Urgente</option>
            </select>
          </div>

          <div className="form-group form-full">
            <label>Observações</label>
            <textarea name="notes" value={form.notes} onChange={handleChange} rows={3} />
          </div>
          {error && <p className="error-msg form-full">{error}</p>}
          <div className="form-full">
            <button type="submit" className="btn-primary" disabled={loading}>
              {loading ? 'Criando...' : 'Criar Solicitação'}
            </button>
          </div>

        </form>
      </div>
    </div>
  );
}
