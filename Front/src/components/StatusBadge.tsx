const STATUS_COLORS: Record<string, string> = {
  Aberta: '#3b82f6',
  Atribuida: '#8b5cf6',
  EmAndamento: '#f59e0b',
  Concluida: '#10b981',
  FalhaNaColeta: '#ef4444',
  AguardandoDecisao: '#ea580c',
  Cancelada: '#6b7280',
};

const STATUS_LABELS: Record<string, string> = {
  Aberta: 'Aberta',
  Atribuida: 'Atribuída',
  EmAndamento: 'Em Andamento',
  Concluida: 'Concluída',
  FalhaNaColeta: 'Falha na Coleta',
  AguardandoDecisao: 'Aguardando Decisão',
  Cancelada: 'Cancelada',
};

export function StatusBadge({ status }: { status: string }) {
  return (
    <span
      style={{
        backgroundColor: STATUS_COLORS[status] ?? '#6b7280',
        color: '#fff',
        padding: '3px 10px',
        borderRadius: 12,
        fontSize: 12,
        fontWeight: 600,
        whiteSpace: 'nowrap',
      }}
    >
      {STATUS_LABELS[status] ?? status}
    </span>
  );
}
