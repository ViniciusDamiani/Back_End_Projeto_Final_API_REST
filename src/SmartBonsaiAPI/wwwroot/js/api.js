// Funções de comunicação com a API

class SmartBonsaiAPI {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }

    async request(endpoint, options = {}) {
        try {
            const response = await fetch(`${this.baseUrl}${endpoint}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                },
                ...options
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('API Request Error:', error);
            throw error;
        }
    }

    // Measurements
    async getLatestMeasurement(deviceId) {
        return this.request(`/api/devices/${deviceId}/measurements/latest`);
    }

    async getMeasurementHistory(deviceId, limit = 100) {
        return this.request(`/api/devices/${deviceId}/measurements/history?limit=${limit}`);
    }

    async createMeasurement(deviceId, data) {
        return this.request(`/api/devices/${deviceId}/measurements`, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }

    // Actuators
    async executeCommand(actuatorId, action, value = null) {
        const body = { action };
        if (value !== null) {
            body.value = value;
        }
        return this.request(`/api/actuators/${actuatorId}/commands`, {
            method: 'POST',
            body: JSON.stringify(body)
        });
    }

    async getActuatorStatus(actuatorId) {
        return this.request(`/api/actuators/${actuatorId}/status`);
    }

    // Automation
    async getAutomationStatus() {
        return this.request('/api/automation/status');
    }

    async enableAutomation() {
        return this.request('/api/automation/enable', {
            method: 'POST'
        });
    }

    async disableAutomation() {
        return this.request('/api/automation/disable', {
            method: 'POST'
        });
    }

    async updateAutomationTargets(targets) {
        return this.request('/api/automation/targets', {
            method: 'PUT',
            body: JSON.stringify(targets)
        });
    }

    async evaluateAutomation() {
        return this.request('/api/automation/evaluate', {
            method: 'POST'
        });
    }

    // Weather API (OpenWeatherMap)
    async getWeather() {
        if (!CONFIG.WEATHER_API_KEY) {
            // Retornar dados mock se não houver API key
            return {
                name: CONFIG.WEATHER_CITY.split(',')[0],
                main: {
                    temp: 22,
                    humidity: 65
                },
                weather: [{
                    main: 'Clear',
                    description: 'Céu limpo'
                }]
            };
        }

        try {
            const response = await fetch(
                `https://api.openweathermap.org/data/2.5/weather?q=${CONFIG.WEATHER_CITY}&appid=${CONFIG.WEATHER_API_KEY}&units=metric&lang=pt_br`
            );
            return await response.json();
        } catch (error) {
            console.error('Weather API Error:', error);
            return null;
        }
    }
}

// Instância global da API
const api = new SmartBonsaiAPI(CONFIG.API_BASE_URL);

