// src/SmartBonsaiAPI/wwwroot/js/config.js
(function() {
  window.CONFIG = {
    API_BASE_URL: window.location.origin,
    DEVICE_IDS: { SOIL: 1, AIR: 2, FLOW: 4 },
    ACTUATOR_IDS: {
      WATER_PUMP: '00000000-0000-0000-0000-000000000001',
      UV_LIGHT:   '00000000-0000-0000-0000-000000000002',
      FAN:        '00000000-0000-0000-0000-000000000003',
      BUZZER:     '00000000-0000-0000-0000-000000000004'
    },
    UPDATE_INTERVAL: 5000,
    WEATHER_API_KEY: null,
    WEATHER_CITY: 'Criciuma,BR'
  };
})();

// IDs dos atuadores podem ser configurados aqui ou obtidos dinamicamente da API
