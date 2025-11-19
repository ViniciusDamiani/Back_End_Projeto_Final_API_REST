// Configurações do Dashboard
require('dotenv').config();

const CONFIG = {
    API_BASE_URL: window.location.origin, // Usar a mesma origem
    DEVICE_IDS: {
        SOIL: 1,      // Dispositivo de umidade do solo
        AIR: 2        // Dispositivo de temperatura e umidade do ar
    },
    ACTUATOR_IDS: {
        WATER_PUMP: '00000000-0000-0000-0000-000000000001',  // Bomba de água
        UV_LIGHT: '00000000-0000-0000-0000-000000000002',    // Iluminação UV
        FAN: '00000000-0000-0000-0000-000000000003'          // Ventilador
    },
    UPDATE_INTERVAL: 5000, // Atualizar a cada 5 segundos
    WEATHER_API_KEY: process.env.WEATHER_API_KEY, // Adicionar chave da API de clima se necessário
    WEATHER_CITY: 'Florianópolis,BR' // Cidade padrão
};

// IDs dos atuadores podem ser configurados aqui ou obtidos dinamicamente da API

