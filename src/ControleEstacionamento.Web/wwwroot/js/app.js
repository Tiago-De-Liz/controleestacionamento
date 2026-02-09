// Auto-refresh functionality
let refreshInterval = null;

function startAutoRefresh(intervalMs = 60000) {
    if (refreshInterval) {
        clearInterval(refreshInterval);
    }

    refreshInterval = setInterval(() => {
        refreshVeiculosTable();
    }, intervalMs);

    updateRefreshStatus(true);
}

function stopAutoRefresh() {
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }
    updateRefreshStatus(false);
}

function toggleAutoRefresh() {
    if (refreshInterval) {
        stopAutoRefresh();
    } else {
        startAutoRefresh();
    }
}

function updateRefreshStatus(active) {
    const btn = document.getElementById('btnAutoRefresh');
    const icon = document.getElementById('refreshIcon');
    if (btn) {
        if (active) {
            btn.classList.remove('bg-gray-200', 'text-gray-700');
            btn.classList.add('bg-green-100', 'text-green-700');
            btn.title = 'Auto-refresh ativo (clique para desativar)';
            if (icon) icon.classList.add('animate-spin');
        } else {
            btn.classList.remove('bg-green-100', 'text-green-700');
            btn.classList.add('bg-gray-200', 'text-gray-700');
            btn.title = 'Auto-refresh inativo (clique para ativar)';
            if (icon) icon.classList.remove('animate-spin');
        }
    }
}

async function refreshVeiculosTable() {
    try {
        const response = await fetch('/Veiculos/ListarEstacionadosPartial');
        if (response.ok) {
            const html = await response.text();
            const container = document.getElementById('veiculosTableContainer');
            if (container) {
                container.innerHTML = html;
            }
        }
    } catch (error) {
        console.error('Erro ao atualizar tabela:', error);
    }
}

function manualRefresh() {
    refreshVeiculosTable();
    showToast('Dados atualizados!', 'success');
}

// Modal functionality
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('hidden');
        modal.classList.add('flex');
        document.body.classList.add('overflow-hidden');
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
        document.body.classList.remove('overflow-hidden');
    }
}

// Saída modal
async function confirmarSaida(veiculoId, placa) {
    document.getElementById('saidaPlaca').textContent = placa;
    document.getElementById('saidaVeiculoId').value = veiculoId;
    document.getElementById('saidaResultado').classList.add('hidden');
    document.getElementById('saidaConfirmacao').classList.remove('hidden');
    document.getElementById('btnConfirmarSaida').disabled = false;
    openModal('modalSaida');
}

async function processarSaida() {
    const veiculoId = document.getElementById('saidaVeiculoId').value;
    const btn = document.getElementById('btnConfirmarSaida');

    btn.disabled = true;
    btn.innerHTML = '<svg class="animate-spin h-5 w-5 mr-2 inline" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg> Processando...';

    try {
        const formData = new FormData();
        formData.append('id', veiculoId);

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        const response = await fetch('/Veiculos/RegistrarSaidaAjax', {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            document.getElementById('saidaConfirmacao').classList.add('hidden');
            document.getElementById('saidaResultado').classList.remove('hidden');

            document.getElementById('resultPlaca').textContent = result.placa;
            document.getElementById('resultEntrada').textContent = result.entrada;
            document.getElementById('resultSaida').textContent = result.saida;
            document.getElementById('resultTempo').textContent = result.tempo;
            document.getElementById('resultValor').textContent = result.valor;

            // Atualiza a tabela em background
            refreshVeiculosTable();
        } else {
            closeModal('modalSaida');
            showToast(result.message || 'Erro ao registrar saída', 'error');
        }
    } catch (error) {
        closeModal('modalSaida');
        showToast('Erro ao processar saída', 'error');
        console.error(error);
    }
}

function fecharSaidaEAtualizar() {
    closeModal('modalSaida');
    location.reload();
}

// Toast notifications
function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer') || createToastContainer();

    const toast = document.createElement('div');
    toast.className = `transform transition-all duration-300 ease-out translate-x-full`;

    const colors = {
        success: 'bg-green-500',
        error: 'bg-red-500',
        info: 'bg-blue-500',
        warning: 'bg-yellow-500'
    };

    toast.innerHTML = `
        <div class="${colors[type] || colors.info} text-white px-6 py-3 rounded-lg shadow-lg flex items-center space-x-3">
            <span>${message}</span>
            <button onclick="this.parentElement.parentElement.remove()" class="text-white/80 hover:text-white">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                </svg>
            </button>
        </div>
    `;

    container.appendChild(toast);

    // Animate in
    setTimeout(() => {
        toast.classList.remove('translate-x-full');
        toast.classList.add('translate-x-0');
    }, 10);

    // Auto remove
    setTimeout(() => {
        toast.classList.add('translate-x-full');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toastContainer';
    container.className = 'fixed top-4 right-4 z-50 space-y-2';
    document.body.appendChild(container);
    return container;
}

// Histórico filters
function filtrarHistorico() {
    const form = document.getElementById('filtroHistoricoForm');
    if (form) {
        form.submit();
    }
}

function limparFiltros() {
    window.location.href = '/Veiculos/Historico';
}

// Export functionality
async function exportarCSV() {
    const params = new URLSearchParams(window.location.search);
    window.location.href = '/Veiculos/ExportarCSV?' + params.toString();
}

// Chart initialization (will be called from the view)
function initOcupacaoChart(data) {
    const ctx = document.getElementById('ocupacaoChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [{
                label: 'Veículos',
                data: data.valores,
                borderColor: 'rgb(59, 130, 246)',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        stepSize: 1
                    }
                }
            }
        }
    });
}

function initReceitaChart(data) {
    const ctx = document.getElementById('receitaChart');
    if (!ctx) return;

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.labels,
            datasets: [{
                label: 'Receita (R$)',
                data: data.valores,
                backgroundColor: 'rgba(34, 197, 94, 0.8)',
                borderColor: 'rgb(34, 197, 94)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'R$ ' + value.toFixed(2);
                        }
                    }
                }
            }
        }
    });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    // Start auto-refresh if on vehicles page
    if (document.getElementById('veiculosTableContainer')) {
        startAutoRefresh();
    }
});
