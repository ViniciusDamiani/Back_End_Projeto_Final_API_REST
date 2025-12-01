<<<<<<< HEAD
// Configurações do Dashboard
const CONFIG = {
    API_BASE_URL: window.location.origin, // Usar a mesma origem
    DEVICE_IDS: {
        SOIL: 1,      // Dispositivo de umidade do solo
        AIR: 2,       // Dispositivo de temperatura e umidade do ar
        LIGHT: 3      // Dispositivo de luminosidade
    },
=======
// src/SmartBonsaiAPI/wwwroot/js/config.js
(function() {
  window.CONFIG = {
    API_BASE_URL: window.location.origin,
    DEVICE_IDS: { SOIL: 1, AIR: 2, FLOW: 4 },
>>>>>>> upstream/main
    ACTUATOR_IDS: {
      WATER_PUMP: '00000000-0000-0000-0000-000000000001',
      UV_LIGHT:   '00000000-0000-0000-0000-000000000002',
      FAN:        '00000000-0000-0000-0000-000000000003',
      BUZZER:     '00000000-0000-0000-0000-000000000004'
    },
<<<<<<< HEAD
    UPDATE_INTERVAL: 5000, // Atualizar a cada 5 segundos
    WEATHER_API_KEY: '', // Será carregado do backend
    WEATHER_CITY: 'Criciúma,BR' // Cidade padrão
};
=======
    UPDATE_INTERVAL: 5000,
    WEATHER_API_KEY: null,
    WEATHER_CITY: 'Criciuma,BR'
  };
})();
>>>>>>> upstream/main

// Carregar configurações do backend
async function loadConfig() {
    try {
        const response = await fetch(`${CONFIG.API_BASE_URL}/api/info/config`);
        if (response.ok) {
            const config = await response.json();
            CONFIG.WEATHER_API_KEY = config.weatherApiKey || '';
            CONFIG.WEATHER_CITY = config.weatherCity || 'Criciúma,BR';
        }
    } catch (error) {
        console.warn('Não foi possível carregar configurações do backend:', error);
    }
}

// Carregar configurações quando o script for carregado
loadConfig();

// IDs dos atuadores podem ser configurados aqui ou obtidos dinamicamente da API
