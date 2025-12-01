// Lógica principal do Dashboard

class Dashboard {
    constructor() {
        this.updateInterval = null;
        this.lastBuzzerOn = null;
        this.alarmAck = false;
        this.lastStatus = null;
        this.waterOn = null;
        this.aeratorOn = null;
        this.scheduledTimes = JSON.parse(localStorage.getItem('waterSchedule') || '["07:00","19:00"]');
        this.lastScheduleRun = localStorage.getItem('waterScheduleLastRun') || null;
        this.automationEnabled = false;
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

                navLinks.forEach(l => l.classList.remove('active'));
                pages.forEach(p => p.classList.remove('active'));

                link.classList.add('active');
                document.getElementById(`page-${targetPage}`)?.classList.add('active');

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
        document.getElementById('btn-water-on')?.addEventListener('click', () => {
            this.turnOnWater();
        });

        document.getElementById('btn-water-toggle')?.addEventListener('click', () => {
            this.toggleWater();
        });

        document.getElementById('btn-aerator-toggle')?.addEventListener('click', () => {
            this.toggleAerator();
        });

        document.getElementById('btn-auto-enable')?.addEventListener('click', () => {
            this.enableAutomation();
        });

        document.getElementById('btn-auto-disable')?.addEventListener('click', () => {
            this.disableAutomation();
        });

        document.getElementById('btn-light-toggle')?.addEventListener('click', () => {
            this.toggleUVLight();
        });

        document.getElementById('btn-save-schedule')?.addEventListener('click', () => {
            this.saveWaterSchedule();
        });

        document.getElementById('btn-save-targets')?.addEventListener('click', () => {
            this.saveAutomationTargets();
        });

        document.getElementById('btn-load-history')?.addEventListener('click', () => {
            this.loadHistoryPage();
        });

        document.getElementById('btn-ack-alarm')?.addEventListener('click', () => {
            this.alarmAck = true;
            this.setAlarmUi(true, true);
            // força desligar buzzer ao reconhecer
            this.updateBuzzer(false, true);
        });
    }

    async loadInitialData() {
        await Promise.all([
            this.updateSensorData(),
            this.updateAutomationStatus(),
            this.updateWateringHistory(),
            this.updateWaterCard(),
            this.updateWeather(),
            this.updatePlantStatus(),
            this.refreshActuatorStates()
        ]);
    }

    startUpdates() {
        this.updateInterval = setInterval(() => {
            this.updateSensorData();
            this.updateAutomationStatus();
            this.updatePlantStatus();
            this.updateWaterCard();
            this.updateWateringHistory();
            this.refreshActuatorStates();
            this.checkScheduledWatering();
        }, CONFIG.UPDATE_INTERVAL);

        setInterval(() => {
            this.updateWeather();
        }, 600000);
    }

    async updateSensorData() {
        try {
            const [soilData, airData] = await Promise.all([
                api.getLatestMeasurement(CONFIG.DEVICE_IDS.SOIL).catch(() => null),
                api.getLatestMeasurement(CONFIG.DEVICE_IDS.AIR).catch(() => null)
            ]);

            if (airData) {
                const temp = airData.temperatureC || 0;
                document.getElementById('temp-value').textContent = `${temp.toFixed(1)}°C`;
                chartManager.updateTempChart(temp);
                const tempStatus = document.getElementById('temp-status');
                tempStatus.className = 'sensor-status active';
                tempStatus.title = 'Sensor ativo';
            } else {
                document.getElementById('temp-value').textContent = '--°C';
                document.getElementById('temp-status').className = 'sensor-status inactive';
            }

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

            const lightData = soilData || airData;
            const lightCard = document.getElementById('light-card');
            if (lightData) {
                const light = lightData.lightPct || 0;
                document.getElementById('light-value').textContent = `${light.toFixed(1)}%`;
                chartManager.updateLightChart(light);
                document.getElementById('light-status').className = 'sensor-status active';
                if (lightCard) lightCard.style.display = light > 5 ? 'block' : 'none';
            } else {
                document.getElementById('light-value').textContent = '--%';
                document.getElementById('light-status').className = 'sensor-status inactive';
                if (lightCard) lightCard.style.display = 'none';
            }

        } catch (error) {
            console.error('Erro ao atualizar dados dos sensores:', error);
            this.showNotification('Erro ao atualizar dados dos sensores', 'error');
        }
    }

    // Atualiza card/gráfico de consumo de água com base no device de fluxo
    async updateWaterCard() {
        const waterValueEl = document.getElementById('water-value');
        const waterStatusEl = document.getElementById('water-status');

        const resetWaterUi = () => {
            if (waterValueEl) waterValueEl.textContent = '-- ml';
            if (waterStatusEl) waterStatusEl.className = 'sensor-status inactive';
            if (chartManager.charts.water) {
                chartManager.charts.water.data.datasets[0].data = [0, 0, 0, 0, 0, 0, 0];
                chartManager.charts.water.update();
            }
        };

        try {
            const measurements = await api.getMeasurementHistory(CONFIG.DEVICE_IDS.FLOW, 100);
            if (!measurements || measurements.length === 0) {
                resetWaterUi();
                return;
            }

            const totalVolume = measurements.reduce((sum, m) => sum + (m.waterVolumeMl || 0), 0);
            if (waterValueEl) waterValueEl.textContent = `${totalVolume.toFixed(0)} ml`;
            if (waterStatusEl) waterStatusEl.className = 'sensor-status active';

            const perDay = [0, 0, 0, 0, 0, 0, 0]; // Seg..Dom
            measurements.forEach(m => {
                const vol = m.waterVolumeMl || 0;
                const d = new Date(m.createdAt);
                const dayIdx = (d.getDay() + 6) % 7; // DOM=0 -> idx 6
                perDay[dayIdx] += vol;
            });

            if (chartManager.charts.water) {
                chartManager.charts.water.data.datasets[0].data = perDay;
                chartManager.charts.water.update();
            }
        } catch (error) {
            console.error('Erro ao atualizar consumo de agua:', error);
            resetWaterUi();
        }
    }

    async updateAutomationStatus() {
        try {
            const status = await api.getAutomationStatus();
            const statusText = document.getElementById('auto-status-text');
            const statusEl = document.getElementById('auto-status');
            this.automationEnabled = !!status.isEnabled;
            
            if (status.isEnabled) {
                statusText.textContent = 'Automático';
                statusEl.style.background = '#d4edda';
                statusEl.style.color = '#155724';
            } else {
                statusText.textContent = 'Manual';
                statusEl.style.background = '#e2e3e5';
                statusEl.style.color = '#383d41';
            }

            // Habilita/desabilita controles manuais conforme modo
            const waterBtn = document.getElementById('btn-water-toggle');
            const aerBtn = document.getElementById('btn-aerator-toggle');
            const manualEnabled = !this.automationEnabled;
            if (waterBtn) waterBtn.disabled = !manualEnabled;
            if (aerBtn) aerBtn.disabled = !manualEnabled;
            if (!manualEnabled) {
                waterBtn?.classList.add('disabled');
                aerBtn?.classList.add('disabled');
            } else {
                waterBtn?.classList.remove('disabled');
                aerBtn?.classList.remove('disabled');
            }
        } catch (error) {
            console.error('Erro ao atualizar status da automação:', error);
        }
    }

    async updateWateringHistory() {
        const tbody = document.getElementById('watering-history-body');
        if (!tbody) return;

        try {
            const measurements = await api.getMeasurementHistory(CONFIG.DEVICE_IDS.FLOW, 50);
            if (!measurements || measurements.length === 0) {
                tbody.innerHTML = '<tr><td colspan="3">Nenhuma rega registrada</td></tr>';
                return;
            }

            const recent = measurements.slice(-10).reverse();
            tbody.innerHTML = recent.map(m => {
                const d = new Date(m.createdAt);
                const vol = (m.waterVolumeMl || 0).toFixed(0);
                return `
                    <tr>
                        <td>${d.toLocaleDateString('pt-BR')}</td>
                        <td>${d.toLocaleTimeString('pt-BR')}</td>
                        <td>${vol} ml</td>
                    </tr>
                `;
            }).join('');
        } catch (error) {
            console.error('Erro ao carregar historico de rega:', error);
            tbody.innerHTML = '<tr><td colspan="3">Erro ao carregar dados</td></tr>';
        }
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
                this.setAlarmUi(false, false);
                await this.updateBuzzer(false);
                return;
            }

            const soilHumidity = soilData?.soilHumidityPct || 0;
            const temp = airData?.temperatureC || 0;
            const light = soilData?.lightPct || airData?.lightPct || 0;

            let status = 'healthy';
            let message = 'Planta saudável';
            let details = [];

            if (soilHumidity < 30) {
                status = 'danger';
                message = 'Precisa de mais água';
                details.push('Umidade do solo muito baixa');
            } else if (soilHumidity > 80) {
                status = 'warning';
                message = 'Umidade do solo alta';
                details.push('Cuidado com excesso de água');
            }

            if (temp < 15) {
                if (status === 'healthy') status = 'warning';
                details.push('Temperatura abaixo do ideal');
            } else if (temp > 30) {
                if (status === 'healthy') status = 'warning';
                details.push('Temperatura acima do ideal');
            }

            // Ajusta mensagem padrão se status mudou
            if (status === 'warning' && message === 'Planta saudável') {
                message = 'Atenção à planta';
            } else if (status === 'danger' && message === 'Planta saudável') {
                message = 'Precisa de atenção';
            }

            messageEl.innerHTML = `<i class="fas fa-${status === 'healthy' ? 'check-circle' : status === 'warning' ? 'exclamation-triangle' : 'times-circle'}"></i> ${message}`;
            messageEl.className = `status-message ${status}`;
            detailsEl.innerHTML = details.length > 0 
                ? details.map(d => `<p>• ${d}</p>`).join('')
                : '<p>• Todas as condições estão dentro dos parâmetros ideais</p>';

            const alarmActive = status !== 'healthy';

            // Se status mudou para um novo alarme, reseta reconhecimento
            if (alarmActive && this.lastStatus !== status) {
                this.alarmAck = false;
            }

            // Se voltou a saudável, limpa tudo e força buzzer off
            if (!alarmActive) {
                this.alarmAck = false;
                this.setAlarmUi(false, false);
                await this.updateBuzzer(false, true);
                this.lastStatus = status;
                return;
            }

            this.setAlarmUi(alarmActive, this.alarmAck);

            // Aciona buzzer via API conforme status (somente se não reconhecido)
            await this.updateBuzzer(alarmActive && !this.alarmAck);

            this.lastStatus = status;

        } catch (error) {
            console.error('Erro ao atualizar status da planta:', error);
        }
    }

    async updateBuzzer(shouldBeOn, force = false) {
        if (!CONFIG.ACTUATOR_IDS.BUZZER) return;
        if (!force && this.lastBuzzerOn === shouldBeOn) return; // evita chamadas repetidas
        this.lastBuzzerOn = shouldBeOn;
        try {
            await api.executeCommand(CONFIG.ACTUATOR_IDS.BUZZER, shouldBeOn ? 'on' : 'off');
        } catch (error) {
            console.error('Erro ao acionar buzzer:', error);
            this.showNotification('Erro ao acionar buzzer: ' + error.message, 'error');
        }
    }

    setAlarmUi(active, acknowledged) {
        const badgeActive = document.getElementById('alarm-active-badge');
        const badgeAck = document.getElementById('alarm-ack-badge');
        const btnAck = document.getElementById('btn-ack-alarm');

        if (!badgeActive || !badgeAck || !btnAck) return;

        if (!active) {
            badgeActive.style.display = 'none';
            badgeAck.style.display = 'none';
            btnAck.style.display = 'none';
            return;
        }

        if (acknowledged) {
            badgeActive.style.display = 'none';
            badgeAck.style.display = 'inline-flex';
            btnAck.style.display = 'none';
        } else {
            badgeActive.style.display = 'inline-flex';
            badgeAck.style.display = 'none';
            btnAck.style.display = 'inline-flex';
        }
    }

    async toggleWater() {
        const desired = !this.waterOn;
        try {
            await api.executeCommand(CONFIG.ACTUATOR_IDS.WATER_PUMP, desired ? 'on' : 'off');
            this.waterOn = desired;
            this.updateMotorButtons();
            this.showNotification(desired ? 'Irrigação ligada' : 'Irrigação desligada', 'success');
        } catch (error) {
            this.showNotification('Erro ao alternar irrigação: ' + error.message, 'error');
        }
    }

    async toggleAerator() {
        const desired = !this.aeratorOn;
        try {
            await api.executeCommand(CONFIG.ACTUATOR_IDS.FAN, desired ? 'on' : 'off');
            this.aeratorOn = desired;
            this.updateMotorButtons();
            this.showNotification(desired ? 'Aerador ligado' : 'Aerador desligado', 'success');
        } catch (error) {
            this.showNotification('Erro ao alternar aerador: ' + error.message, 'error');
        }
    }

    async refreshActuatorStates() {
        try {
            const [waterStatus, aerStatus] = await Promise.all([
                api.getActuatorStatus(CONFIG.ACTUATOR_IDS.WATER_PUMP).catch(() => null),
                api.getActuatorStatus(CONFIG.ACTUATOR_IDS.FAN).catch(() => null)
            ]);
            if (waterStatus) this.waterOn = !!waterStatus.isActive;
            if (aerStatus) this.aeratorOn = !!aerStatus.isActive;
            this.updateMotorButtons();
        } catch (error) {
            // silencioso
        }
    }

    updateMotorButtons() {
        const waterBtn = document.getElementById('btn-water-toggle');
        const aerBtn = document.getElementById('btn-aerator-toggle');
        if (waterBtn && this.waterOn !== null) {
            waterBtn.className = this.waterOn ? 'btn btn-danger' : 'btn btn-primary';
            waterBtn.innerHTML = this.waterOn
                ? '<i class="fas fa-stop"></i> Desligar Irrigação'
                : '<i class="fas fa-play"></i> Ligar Irrigação';
        }
        if (aerBtn && this.aeratorOn !== null) {
            aerBtn.className = this.aeratorOn ? 'btn btn-secondary' : 'btn btn-success';
            aerBtn.innerHTML = this.aeratorOn
                ? '<i class="fas fa-hand-paper"></i> Desligar Aerador'
                : '<i class="fas fa-fan"></i> Ligar Aerador';
        }
    }

    async checkScheduledWatering() {
        if (!this.automationEnabled) return;
        if (!this.scheduledTimes || this.scheduledTimes.length === 0) return;

        const now = new Date();
        const hh = String(now.getHours()).padStart(2, '0');
        const mm = String(now.getMinutes()).padStart(2, '0');
        const current = `${hh}:${mm}`;
        const todayKey = `${now.getFullYear()}-${now.getMonth()}-${now.getDate()}-${current}`;

        if (this.lastScheduleRun === todayKey) return;
        if (!this.scheduledTimes.includes(current)) return;

        // marca que executou este horário hoje
        this.lastScheduleRun = todayKey;
        localStorage.setItem('waterScheduleLastRun', todayKey);

        try {
            await api.executeCommand(CONFIG.ACTUATOR_IDS.WATER_PUMP, 'on');
            this.waterOn = true;
            this.updateMotorButtons();
            this.showNotification(`Irrigação automática às ${current}`, 'success');
            setTimeout(async () => {
                try {
                    await api.executeCommand(CONFIG.ACTUATOR_IDS.WATER_PUMP, 'off');
                    this.waterOn = false;
                    this.updateMotorButtons();
                } catch (_) { /* ignore */ }
            }, 3000);
        } catch (error) {
            this.showNotification('Erro na irrigação automática: ' + error.message, 'error');
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
        this.scheduledTimes = [time1, time2];
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

document.addEventListener('DOMContentLoaded', () => {
    window.dashboard = new Dashboard();
});
