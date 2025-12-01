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
            const cityName = CONFIG.WEATHER_CITY.split(',')[0];
            return {
                name: cityName,
                main: {
                    temp: 22,
                    humidity: 65,
                    feels_like: 22,
                    temp_min: 18,
                    temp_max: 26,
                    pressure: 1013
                },
                weather: [{
                    main: 'Clear',
                    description: 'Céu limpo',
                    icon: '01d'
                }],
                wind: {
                    speed: 2.5
                }
            };
        }

        try {
            const response = await fetch(
                `https://api.openweathermap.org/data/2.5/weather?q=${encodeURIComponent(CONFIG.WEATHER_CITY)}&appid=${CONFIG.WEATHER_API_KEY}&units=metric&lang=pt_br`
            );
            
            if (!response.ok) {
                throw new Error(`Weather API error: ${response.status} ${response.statusText}`);
            }
            
            const data = await response.json();
            
            // Verificar se a resposta contém erro
            if (data.cod && data.cod !== 200) {
                throw new Error(data.message || 'Erro na API de clima');
            }
            
            return data;
        } catch (error) {
            console.error('Weather API Error:', error);
            // Retornar dados mock em caso de erro
            const cityName = CONFIG.WEATHER_CITY.split(',')[0];
            return {
                name: cityName,
                main: {
                    temp: 22,
                    humidity: 65,
                    feels_like: 22,
                    temp_min: 18,
                    temp_max: 26,
                    pressure: 1013
                },
                weather: [{
                    main: 'Clear',
                    description: 'Dados não disponíveis',
                    icon: '01d'
                }],
                wind: {
                    speed: 2.5
                },
                _error: true,
                _errorMessage: error.message
            };
        }
    }

    // Email testing methods
    async getEmailStatus() {
        return this.request('/api/email/status');
    }

    async sendTestEmail() {
        return this.request('/api/email/test', {
            method: 'POST'
        });
    }

    async sendEmail(subject, body, isHtml = false) {
        return this.request('/api/email/send', {
            method: 'POST',
            body: JSON.stringify({ subject, body, isHtml })
        });
    }
}

// Instância global da API
const api = new SmartBonsaiAPI(CONFIG.API_BASE_URL);


