# Herramientas MCP para Planificación de Itinerarios

Un servidor MCP (Model Context Protocol) que proporciona herramientas inteligentes para la planificación de viajes, incluyendo sugerencias de hoteles y vuelos.

## 🚀 Características Principales

- **Búsqueda de Hoteles**: Sugerencias basadas en ubicación y fechas
- **Búsqueda de Vuelos**: Opciones de vuelos directos y con conexión
- **Validación de Fechas**: Verificación automática de formatos ISO
- **Datos Realistas**: Generación de información mock detallada
- **API Asíncrona**: Soporte completo para async/await
- **Protocolo MCP**: Integración nativa con clientes MCP

## 📋 Herramientas Disponibles

### 🏨 suggest_hotels
Encuentra hoteles perfectos para tu estadía.

**Parámetros:**
- `location`: Ciudad o área de búsqueda
- `check_in`: Fecha de entrada (YYYY-MM-DD)
- `check_out`: Fecha de salida (YYYY-MM-DD)

**Retorna:** Lista de hoteles con ratings, precios, amenidades y disponibilidad.

### ✈️ suggest_flights
Busca vuelos para tus destinos favoritos.

**Parámetros:**
- `from_location`: Ciudad/aeropuerto de origen
- `to_location`: Ciudad/aeropuerto de destino  
- `departure_date`: Fecha de salida (YYYY-MM-DD)
- `return_date`: Fecha de regreso (opcional, YYYY-MM-DD)

**Retorna:** Vuelos de ida y vuelta con horarios, precios y detalles de conexión.

## 🛠️ Instalación y Uso

### Prerrequisitos
- Python 3.12+
- pip o uv

### Instalación
```bash
pip install -e .
# o con uv
uv pip install -e .
```

### Ejecutar el Servidor
```bash
python src/app.py
# o con uv
uv run src/app.py
```

El servidor estará disponible en `http://localhost:8000`

### Ejecutar Pruebas
```bash
pip install -e ".[test]"
pytest tests/ -v
```

## 🔧 Desarrollo y Debug

### Entorno Local con uv
1. Crear entorno virtual:
   ```bash
   uv venv
   ```

2. Instalar dependencias:
   ```bash
   uv pip install -e .
   ```

3. Ejecutar servidor:
   ```shell
   uv run src/app.py
   ```

### Debug con MCP Inspector
Para testing y debugging de funcionalidad MCP:

```cmd
uv run mcp dev src/mcp_server.py
```

## 📚 Documentación Completa

Para documentación detallada en español, incluyendo diagramas de arquitectura, dependencias y guías de desarrollo, consulta [DOCUMENTACION.md](./DOCUMENTACION.md).

## 🔗 Endpoints MCP

- **SSE**: `GET /sse` - Conexión Server-Sent Events para MCP
- **Mensajes**: `POST /messages/` - Manejo de mensajes MCP
- **Web**: `GET /` - Página principal informativa

## 📊 Ejemplo de Respuesta

### Hoteles
```json
[
  {
    "name": "Luxury Hotel Plaza",
    "rating": 4.5,
    "price_per_night": 285,
    "amenities": ["Free WiFi", "Pool", "Spa"],
    "available_rooms": 12
  }
]
```

### Vuelos
```json
{
  "departure_flights": [
    {
      "airline": "SkyWings",
      "flight_number": "SW1234",
      "departure": "2024-12-20T08:30:00",
      "price": 299.99,
      "is_direct": true
    }
  ]
}
```

## 🐳 Docker

```bash
docker build -t itinerary-planning-mcp .
docker run -p 8000:8000 itinerary-planning-mcp
```

## 🧪 Cobertura de Pruebas

- ✅ 25 pruebas unitarias
- ✅ Validación de fechas
- ✅ Generación de datos mock
- ✅ Manejo de errores
- ✅ Casos extremos

## 📈 Estado del Proyecto

- **Pruebas**: ✅ 25/25 pasando
- **Cobertura**: Alta cobertura de funciones críticas
- **Documentación**: Completa en español
- **Listo para**: Desarrollo y demostración
