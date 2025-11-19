// Lógica principal do Dashboard

class Dashboard {
    constructor() {
        this.updateInterval = null;
        this.wateringHistory = JSON.parse(localStorage.getItem('wateringHistory') || '[]');
        this.waterConsumption = JSON.parse(localStorage.getItem('waterConsumption') || '{}');
        this.init();
    }

    init() {
        this.setupNavigation();
        this.initCharts();
        this.setupEventListeners();
        this.startUpdates();
        this.loadInitialData();
    }

    setupNavigation() {
        const navLinks = document.querySelectorAll('.nav-link');
        const pages = document.querySelectorAll('.page');

        navLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetPage = link.dataset.page;

                // Remove active de todos
                navLinks.forEach(l => l.classList.remove('active'));
                pages.forEach(p => p.classList.remove('active'));

                // Adiciona active ao selecionado
                link.classList.add('active');
                document.getElementById(`page-${targetPage}`)?.classList.add('active');

                // Carrega dados específicos da página
                if (targetPage === 'history') {
                    this.loadHistoryPage();
                } else if (targetPage === 'settings') {
                    this.loadSettingsPage();
                }
            });
        });
    }

    initCharts() {
        chartManager.initTempChart('temp-chart');
        chartManager.initSoilGauge('soil-gauge');
        chartManager.initLightChart('light-chart');
        chartManager.initWaterChart('water-chart');
    }

    setupEventListeners() {
        // Controles de água
        document.getElementById('btn-water-on')?.addEventListener('click', () => {
            this.turnOnWater();
        });

        document.getElementById('btn-water-off')?.addEventListener('click', () => {
            this.turnOffWater();
        });

        // Automação
        document.getElementById('btn-auto-enable')?.addEventListener('click', () => {
            this.enableAutomation();
        });

        document.getElementById('btn-auto-disable')?.addEventListener('click', () => {
            this.disableAutomation();
        });

        // Iluminação UV
        document.getElementById('btn-light-toggle')?.addEventListener('click', () => {
            this.toggleUVLight();
        });

        // Horários de rega
        document.getElementById('btn-save-schedule')?.addEventListener('click', () => {
            this.saveWaterSchedule();
        });

        // Configurações
        document.getElementById('btn-save-targets')?.addEventListener('click', () => {
            this.saveAutomationTargets();
        });

        // Histórico
        document.getElementById('btn-load-history')?.addEventListener('click', () => {
            this.loadHistoryPage();
        });
    }

    async loadInitialData() {
        await Promise.all([
            this.updateSensorData(),
            this.updateAutomationStatus(),
            this.updateWateringHistory(),
            this.updateWeather(),
            this.updatePlantStatus()
        ]);
    }

    startUpdates() {
        // Atualizar a cada intervalo configurado
        this.updateInterval = setInterval(() => {
            this.updateSensorData();
            this.updateAutomationStatus();
            this.updatePlantStatus();
        }, CONFIG.UPDATE_INTERVAL);

        // Atualizar clima a cada 10 minutos
        setInterval(() => {
            this.updateWeather();
        }, 600000);
    }

    async updateSensorData() {
        try {
            // Obter medições dos dois dispositivos
            const [soilData, airData] = await Promise.all([
                api.getLatestMeasurement(CONFIG.DEVICE_IDS.SOIL).catch(() => null),
                api.getLatestMeasurement(CONFIG.DEVICE_IDS.AIR).catch(() => null)
            ]);

            // Temperatura (dispositivo 2)
            if (airData) {
                const temp = airData.temperatureC || 0;
                document.getElementById('temp-value').textContent = `${temp.toFixed(1)}°C`;
                chartManager.updateTempChart(temp);
                
                // Status do sensor
                const tempStatus = document.getElementById('temp-status');
                tempStatus.className = 'sensor-status active';
                tempStatus.title = 'Sensor ativo';
            } else {
                document.getElementById('temp-value').textContent = '--°C';
                document.getElementById('temp-status').className = 'sensor-status inactive';
            }

            // Umidade do Solo (dispositivo 1)
            if (soilData) {
                const soilHumidity = soilData.soilHumidityPct || 0;
                document.getElementById('soil-value').textContent = `${soilHumidity.toFixed(1)}%`;
                chartManager.updateSoilGauge(soilHumidity);
                document.getElementById('soil-status').className = 'sensor-status active';
            } else {
                document.getElementById('soil-value').textContent = '--%';
                chartManager.updateSoilGauge(0);
                document.getElementById('soil-status').className = 'sensor-status inactive';
            }

            // Luminosidade e Umidade do Ar
            const lightData = soilData || airData;
            if (lightData) {
                const light = lightData.lightPct || 0;
                document.getElementById('light-value').textContent = `${light.toFixed(1)}%`;
                chartManager.updateLightChart(light);
                document.getElementById('light-status').className = 'sensor-status active';
            } else {
                document.getElementById('light-value').textContent = '--%';
                document.getElementById('light-status').className = 'sensor-status inactive';
            }

            if (airData) {
                const humidity = airData.humidityPct || 0;
                // Pode ser usado para outros gráficos se necessário
            }

        } catch (error) {
            console.error('Erro ao atualizar dados dos sensores:', error);
            this.showNotification('Erro ao atualizar dados dos sensores', 'error');
        }
    }

    async updateAutomationStatus() {
        try {
            const status = await api.getAutomationStatus();
            const statusText = document.getElementById('auto-status-text');
            const statusEl = document.getElementById('auto-status');
            
            if (status.isEnabled) {
                statusText.textContent = 'Ativo';
                statusEl.style.background = '#d4edda';
                statusEl.style.color = '#155724';
            } else {
                statusText.textContent = 'Inativo';
                statusEl.style.background = '#f8d7da';
                statusEl.style.color = '#721c24';
            }
        } catch (error) {
            console.error('Erro ao atualizar status da automação:', error);
        }
    }

    async updateWateringHistory() {
        const tbody = document.getElementById('watering-history-body');
        if (!tbody) return;

        if (this.wateringHistory.length === 0) {
            tbody.innerHTML = '<tr><td colspan="3">Nenhuma rega registrada</td></tr>';
            return;
        }

        // Mostrar últimas 10 regas
        const recent = this.wateringHistory.slice(-10).reverse();
        tbody.innerHTML = recent.map(item => `
            <tr>
                <td>${new Date(item.date).toLocaleDateString('pt-BR')}</td>
                <td>${new Date(item.date).toLocaleTimeString('pt-BR')}</td>
                <td>${item.amount} ml</td>
            </tr>
        `).join('');
    }

    async updateWeather() {
        try {
            const weather = await api.getWeather();
            const container = document.getElementById('weather-container');
            
            if (!weather || !container) return;

            container.innerHTML = `
                <div class="weather-info">
                    <h4>${weather.name}</h4>
                    <p><strong>Temperatura:</strong> ${weather.main.temp}°C</p>
                    <p><strong>Umidade:</strong> ${weather.main.humidity}%</p>
                    <p><strong>Condição:</strong> ${weather.weather[0].description}</p>
                </div>
            `;
        } catch (error) {
            console.error('Erro ao atualizar clima:', error);
        }
    }

    async updatePlantStatus() {
        try {
            const [soilData, airData] = await Promise.all([
                api.getLatestMeasurement(CONFIG.DEVICE_IDS.SOIL).catch(() => null),
                api.getLatestMeasurement(CONFIG.DEVICE_IDS.AIR).catch(() => null)
            ]);

            const messageEl = document.getElementById('plant-status-message');
            const detailsEl = document.getElementById('plant-status-details');

            if (!soilData && !airData) {
                messageEl.innerHTML = '<i class="fas fa-question-circle"></i> Dados insuficientes';
                messageEl.className = 'status-message warning';
                detailsEl.textContent = 'Aguardando dados dos sensores...';
                return;
            }

            const soilHumidity = soilData?.soilHumidityPct || 0;
            const temp = airData?.temperatureC || 0;
            const light = soilData?.lightPct || airData?.lightPct || 0;

            let status = 'healthy';
            let message = 'Planta saudável';
            let details = [];

            // Verificar umidade do solo
            if (soilHumidity < 30) {
                status = 'danger';
                message = 'Precisa de mais água';
                details.push('Umidade do solo muito baixa');
            } else if (soilHumidity > 80) {
                status = 'warning';
                message = 'Umidade do solo alta';
                details.push('Cuidado com excesso de água');
            }

            // Verificar temperatura
            if (temp < 15) {
                if (status === 'healthy') status = 'warning';
                details.push('Temperatura abaixo do ideal');
            } else if (temp > 30) {
                if (status === 'healthy') status = 'warning';
                details.push('Temperatura acima do ideal');
            }

            // Verificar luminosidade
            if (light < 30) {
                if (status === 'healthy') status = 'warning';
                details.push('Precisa de mais luz');
            }

            messageEl.innerHTML = `<i class="fas fa-${status === 'healthy' ? 'check-circle' : status === 'warning' ? 'exclamation-triangle' : 'times-circle'}"></i> ${message}`;
            messageEl.className = `status-message ${status}`;
            detailsEl.innerHTML = details.length > 0 
                ? details.map(d => `<p>• ${d}</p>`).join('')
                : '<p>• Todas as condições estão dentro dos parâmetros ideais</p>';

        } catch (error) {
            console.error('Erro ao atualizar status da planta:', error);
        }
    }

    async turnOnWater() {
        try {
            await api.executeCommand(CONFIG.ACTUATOR_IDS.WATER_PUMP, 'on');
            this.showNotification('Rega iniciada', 'success');
            
            // Registrar no histórico
            this.addWateringRecord(50); // Quantidade estimada em ml
        } catch (error) {
            this.showNotification('Erro ao ligar rega: ' + error.message, 'error');
        }
    }

    async turnOffWater() {
        try {
            await api.executeCommand(CONFIG.ACTUATOR_IDS.WATER_PUMP, 'off');
            this.showNotification('Rega desligada', 'success');
        } catch (error) {
            this.showNotification('Erro ao desligar rega: ' + error.message, 'error');
        }
    }

    async enableAutomation() {
        try {
            await api.enableAutomation();
            this.showNotification('Modo automático ativado', 'success');
            this.updateAutomationStatus();
        } catch (error) {
            this.showNotification('Erro ao ativar automação: ' + error.message, 'error');
        }
    }

    async disableAutomation() {
        try {
            await api.disableAutomation();
            this.showNotification('Modo automático desativado', 'warning');
            this.updateAutomationStatus();
        } catch (error) {
            this.showNotification('Erro ao desativar automação: ' + error.message, 'error');
        }
    }

    async toggleUVLight() {
        try {
            const status = await api.getActuatorStatus(CONFIG.ACTUATOR_IDS.UV_LIGHT);
            const action = status?.isActive ? 'off' : 'on';
            await api.executeCommand(CONFIG.ACTUATOR_IDS.UV_LIGHT, action);
            
            const text = action === 'on' ? 'Desligar' : 'Ligar';
            document.getElementById('light-toggle-text').textContent = text;
            this.showNotification(`Iluminação UV ${action === 'on' ? 'ligada' : 'desligada'}`, 'success');
        } catch (error) {
            this.showNotification('Erro ao controlar iluminação: ' + error.message, 'error');
        }
    }

    saveWaterSchedule() {
        const time1 = document.getElementById('schedule-time-1').value;
        const time2 = document.getElementById('schedule-time-2').value;
        
        localStorage.setItem('waterSchedule', JSON.stringify([time1, time2]));
        this.showNotification('Horários de rega salvos', 'success');
    }

    async saveAutomationTargets() {
        try {
            const targets = {
                isEnabled: true,
                targetTemperatureMin: parseFloat(document.getElementById('setting-temp-min').value),
                targetTemperatureMax: parseFloat(document.getElementById('setting-temp-max').value),
                targetHumidityMin: parseFloat(document.getElementById('setting-humidity-min').value),
                targetHumidityMax: parseFloat(document.getElementById('setting-humidity-max').value)
            };

            await api.updateAutomationTargets(targets);
            this.showNotification('Metas de automação salvas', 'success');
        } catch (error) {
            this.showNotification('Erro ao salvar metas: ' + error.message, 'error');
        }
    }

    async loadSettingsPage() {
        try {
            const status = await api.getAutomationStatus();
            document.getElementById('setting-temp-min').value = status.targetTemperatureMin || 18;
            document.getElementById('setting-temp-max').value = status.targetTemperatureMax || 25;
            document.getElementById('setting-humidity-min').value = status.targetHumidityMin || 40;
            document.getElementById('setting-humidity-max').value = status.targetHumidityMax || 70;

            const schedule = JSON.parse(localStorage.getItem('waterSchedule') || '["07:00", "19:00"]');
            if (schedule[0]) document.getElementById('schedule-time-1').value = schedule[0];
            if (schedule[1]) document.getElementById('schedule-time-2').value = schedule[1];
        } catch (error) {
            console.error('Erro ao carregar configurações:', error);
        }
    }

    async loadHistoryPage() {
        const deviceId = parseInt(document.getElementById('history-device-select').value);
        const limit = parseInt(document.getElementById('history-limit').value) || 50;

        try {
            const measurements = await api.getMeasurementHistory(deviceId, limit);
            const tbody = document.getElementById('full-history-body');

            if (!measurements || measurements.length === 0) {
                tbody.innerHTML = '<tr><td colspan="5">Nenhum dado disponível</td></tr>';
                return;
            }

            tbody.innerHTML = measurements.map(m => `
                <tr>
                    <td>${new Date(m.createdAt).toLocaleString('pt-BR')}</td>
                    <td>${m.temperatureC?.toFixed(1) || '--'}</td>
                    <td>${m.soilHumidityPct?.toFixed(1) || '--'}</td>
                    <td>${m.humidityPct?.toFixed(1) || '--'}</td>
                    <td>${m.lightPct?.toFixed(1) || '--'}</td>
                </tr>
            `).join('');

            chartManager.updateHistoryChart(measurements);

        } catch (error) {
            console.error('Erro ao carregar histórico:', error);
            this.showNotification('Erro ao carregar histórico', 'error');
        }
    }

    addWateringRecord(amount) {
        const record = {
            date: new Date().toISOString(),
            amount: amount
        };
        this.wateringHistory.push(record);
        
        // Manter apenas últimas 100 regas
        if (this.wateringHistory.length > 100) {
            this.wateringHistory = this.wateringHistory.slice(-100);
        }
        
        localStorage.setItem('wateringHistory', JSON.stringify(this.wateringHistory));
        this.updateWateringHistory();

        // Atualizar consumo semanal
        const week = this.getWeekOfYear(new Date());
        this.waterConsumption[week] = (this.waterConsumption[week] || 0) + amount;
        localStorage.setItem('waterConsumption', JSON.stringify(this.waterConsumption));
        this.updateWaterChart();
    }

    getWeekOfYear(date) {
        const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
        const dayNum = d.getUTCDay() || 7;
        d.setUTCDate(d.getUTCDate() + 4 - dayNum);
        const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
        return Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
    }

    updateWaterChart() {
        if (!chartManager.charts.water) return;

        const currentWeek = this.getWeekOfYear(new Date());
        const weekData = [0, 0, 0, 0, 0, 0, 0];

        // Simulação de dados (em produção, calcular baseado no histórico real)
        Object.keys(this.waterConsumption).forEach(week => {
            if (Math.abs(parseInt(week) - currentWeek) <= 3) {
                const amount = this.waterConsumption[week] / 7; // Distribuir pelos dias
                weekData.forEach((_, i) => {
                    weekData[i] += amount;
                });
            }
        });

        chartManager.charts.water.data.datasets[0].data = weekData;
        chartManager.charts.water.update();
    }

    showNotification(message, type = 'info') {
        const container = document.getElementById('notifications');
        if (!container) return;

        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.innerHTML = `
            <i class="fas fa-${type === 'error' ? 'exclamation-circle' : type === 'success' ? 'check-circle' : 'info-circle'}"></i>
            <span>${message}</span>
        `;

        container.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideIn 0.3s ease-out reverse';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
}

// Inicializar dashboard quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', () => {
    window.dashboard = new Dashboard();
});

