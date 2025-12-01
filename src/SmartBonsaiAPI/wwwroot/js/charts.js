// Gerenciamento de gráficos usando Chart.js

class ChartManager {
    constructor() {
        this.charts = {};
    }

    // Gráfico de linha para temperatura
    initTempChart(canvasId) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        this.charts.temp = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [{
                    label: 'Temperatura (°C)',
                    data: [],
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    tension: 0.4,
                    fill: true
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
                        beginAtZero: false,
                        title: {
                            display: true,
                            text: '°C'
                        }
                    }
                }
            }
        });
        return this.charts.temp;
    }

    // Medidor circular (gauge) para umidade do solo
    initSoilGauge(canvasId) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        this.charts.soil = new Chart(ctx, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: [0, 100],
                    backgroundColor: ['#28a745', '#e9ecef'],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '75%',
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });
        return this.charts.soil;
    }

    updateSoilGauge(value) {
        if (this.charts.soil) {
            this.charts.soil.data.datasets[0].data = [value, 100 - value];
            const color = value < 30 ? '#dc3545' : value < 60 ? '#ffc107' : '#28a745';
            this.charts.soil.data.datasets[0].backgroundColor = [color, '#e9ecef'];
            this.charts.soil.update();
        }
    }

    // Gráfico de barras para luminosidade
    initLightChart(canvasId) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        this.charts.light = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Luminosidade (%)',
                    data: [],
                    backgroundColor: 'rgba(255, 193, 7, 0.8)',
                    borderColor: '#ffc107',
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
                        max: 100,
                        title: {
                            display: true,
                            text: '%'
                        }
                    }
                }
            }
        });
        return this.charts.light;
    }

    // Gráfico acumulado semanal para consumo de água
    initWaterChart(canvasId) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        this.charts.water = new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'],
                datasets: [{
                    label: 'Consumo Acumulado (ml)',
                    data: [0, 0, 0, 0, 0, 0, 0],
                    borderColor: '#17a2b8',
                    backgroundColor: 'rgba(23, 162, 184, 0.1)',
                    tension: 0.4,
                    fill: true
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
                        title: {
                            display: true,
                            text: 'ml'
                        }
                    }
                }
            }
        });
        return this.charts.water;
    }

    // Gráfico de histórico completo
    initHistoryChart(canvasId) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return null;

        this.charts.history = new Chart(ctx, {
            type: 'line',
            data: {
                labels: [],
                datasets: [
                    {
                        label: 'Temperatura (°C)',
                        data: [],
                        borderColor: '#dc3545',
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        yAxisID: 'y',
                        tension: 0.4
                    },
                    {
                        label: 'Umidade Solo (%)',
                        data: [],
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
                        yAxisID: 'y1',
                        tension: 0.4
                    },
                    {
                        label: 'Umidade Ar (%)',
                        data: [],
                        borderColor: '#17a2b8',
                        backgroundColor: 'rgba(23, 162, 184, 0.1)',
                        yAxisID: 'y1',
                        tension: 0.4
                    },
                    {
                        label: 'Luminosidade (%)',
                        data: [],
                        borderColor: '#ffc107',
                        backgroundColor: 'rgba(255, 193, 7, 0.1)',
                        yAxisID: 'y1',
                        tension: 0.4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                scales: {
                    x: {
                        title: {
                            display: true,
                            text: 'Data/Hora'
                        }
                    },
                    y: {
                        type: 'linear',
                        display: true,
                        position: 'left',
                        title: {
                            display: true,
                            text: 'Temperatura (°C)'
                        }
                    },
                    y1: {
                        type: 'linear',
                        display: true,
                        position: 'right',
                        title: {
                            display: true,
                            text: '%'
                        },
                        grid: {
                            drawOnChartArea: false
                        }
                    }
                }
            }
        });
        return this.charts.history;
    }

    // Atualizar gráfico de temperatura
    updateTempChart(data) {
        if (!this.charts.temp) return;
        
        const chart = this.charts.temp;
        const maxPoints = 20;
        
        chart.data.labels.push(new Date().toLocaleTimeString());
        chart.data.datasets[0].data.push(data);
        
        if (chart.data.labels.length > maxPoints) {
            chart.data.labels.shift();
            chart.data.datasets[0].data.shift();
        }
        
        chart.update('none');
    }

    // Atualizar gráfico de luminosidade
    updateLightChart(data) {
        if (!this.charts.light) return;
        
        const chart = this.charts.light;
        const maxBars = 12;
        
        chart.data.labels.push(new Date().toLocaleTimeString());
        chart.data.datasets[0].data.push(data);
        
        if (chart.data.labels.length > maxBars) {
            chart.data.labels.shift();
            chart.data.datasets[0].data.shift();
        }
        
        chart.update('none');
    }

    // Atualizar histórico
    updateHistoryChart(measurements) {
        if (!this.charts.history || !measurements || measurements.length === 0) return;
        
        const chart = this.charts.history;
        
        chart.data.labels = measurements.map(m => 
            new Date(m.createdAt).toLocaleString('pt-BR', {
                day: '2-digit',
                month: '2-digit',
                hour: '2-digit',
                minute: '2-digit'
            })
        );
        
        chart.data.datasets[0].data = measurements.map(m => m.temperatureC);
        chart.data.datasets[1].data = measurements.map(m => m.soilHumidityPct);
        chart.data.datasets[2].data = measurements.map(m => m.humidityPct);
        chart.data.datasets[3].data = measurements.map(m => m.lightPct);
        
        chart.update();
    }
}

// Instância global do gerenciador de gráficos
const chartManager = new ChartManager();


