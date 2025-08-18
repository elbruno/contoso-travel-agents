# Servidor MCP de Planificación de Itinerarios

Servidor MCP (Model Context Protocol) implementado en Python para la planificación inteligente de itinerarios de viaje.

## Características Principales

- 🏨 **Sugerencias de Hoteles**: Recomendaciones personalizadas basadas en ubicación y fechas
- ✈️ **Sugerencias de Vuelos**: Opciones de vuelos directos y con conexiones
- 📅 **Validación Robusta**: Verificación de fechas en formato ISO
- 🎲 **Datos Realistas**: Generación de información convincente con Faker
- 🧪 **Cobertura Completa**: 96% de cobertura de código con pruebas unitarias

## Entorno Local

1. Crear un [entorno virtual de Python](https://docs.python.org/3/tutorial/venv.html#creating-virtual-environments) y activarlo:

    ```bash
    python -m venv venv
    source venv/bin/activate  # En Windows: venv\Scripts\activate
    ```

2. Instalar el servidor MCP con dependencias de prueba:

    ```bash
    pip install -e .[test]
    ```

3. Ejecutar el servidor MCP:

    ```shell
    python src/app.py
    ```

## Desarrollo y Pruebas

### Ejecutar Pruebas

```bash
# Todas las pruebas con cobertura
python -m pytest tests/ -v --cov=src --cov-report=term-missing

# Solo pruebas específicas
python -m pytest tests/test_mcp_server.py -v

# Pruebas con reporte HTML de cobertura
python -m pytest --cov=src --cov-report=html
```

### Depuración con MCP Inspector

Para probar y depurar la funcionalidad MCP, usa el Inspector de MCP:

```cmd
uv run mcp dev src/mcp_server.py
```

## Herramientas MCP Disponibles

### suggest_hotels

Sugiere hoteles basados en ubicación y fechas.

**Parámetros:**
- `location`: Ubicación (ciudad o área) para buscar hoteles
- `check_in`: Fecha de entrada en formato ISO (YYYY-MM-DD)
- `check_out`: Fecha de salida en formato ISO (YYYY-MM-DD)

**Respuesta:**
```json
[
  {
    "name": "Luxury Hotel",
    "address": "123 Main St",
    "location": "Downtown, Paris",
    "rating": 4.5,
    "price_per_night": 250,
    "hotel_type": "Luxury",
    "amenities": ["Free WiFi", "Pool", "Spa"],
    "available_rooms": 10
  }
]
```

### suggest_flights

Sugiere vuelos basados en ubicaciones y fechas.

**Parámetros:**
- `from_location`: Ubicación de salida (ciudad o aeropuerto)
- `to_location`: Ubicación de destino (ciudad o aeropuerto)
- `departure_date`: Fecha de salida en formato ISO (YYYY-MM-DD)
- `return_date`: (Opcional) Fecha de regreso en formato ISO (YYYY-MM-DD)

**Respuesta:**
```json
{
  "departure_flights": [
    {
      "flight_id": "ABC12345",
      "airline": "SkyWings",
      "flight_number": "SW1234",
      "from_airport": {"code": "LAX", "name": "Los Angeles International"},
      "to_airport": {"code": "JFK", "name": "John F. Kennedy International"},
      "departure": "2024-06-01T10:30:00",
      "arrival": "2024-06-01T18:45:00",
      "is_direct": true,
      "price": 299.99
    }
  ],
  "return_flights": []
}
```

## Estructura del Proyecto

```
src/tools/itinerary-planning/
├── src/
│   ├── app.py                 # Aplicación principal Starlette
│   ├── app_routes.py         # Configuración de rutas HTTP/SSE
│   └── mcp_server.py         # Herramientas MCP y lógica de negocio
├── tests/
│   ├── test_app.py           # Pruebas de la aplicación web
│   └── test_mcp_server.py    # Pruebas de herramientas MCP
├── pyproject.toml            # Configuración del proyecto y dependencias
├── pytest.ini               # Configuración de pytest
└── README.md                 # Esta documentación
```

## Dependencias Principales

- **mcp[cli]**: SDK oficial de Model Context Protocol
- **starlette**: Framework web asíncrono de alto rendimiento
- **uvicorn**: Servidor ASGI para aplicaciones Python
- **faker**: Generación de datos de prueba realistas
- **pydantic**: Validación y serialización de datos

Para más detalles sobre la arquitectura y documentación técnica completa, consulta la [documentación principal](../customer-query/README.md).
