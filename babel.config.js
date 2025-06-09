module.exports = function(api) {
  api.cache(true);
  return {
    presets: ['babel-preset-expo'],
    plugins: [
      // Configuração específica para o Reanimated
      'react-native-reanimated/plugin',
    ],
  };
}; 