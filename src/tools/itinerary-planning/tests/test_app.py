import pytest
from unittest.mock import AsyncMock, patch
from starlette.testclient import TestClient
from starlette.applications import Starlette
from starlette.responses import Response

from src.app import app
from src.app_routes import routes, homepage, handle_sse


class TestApp:
    """Test cases for the main Starlette application."""

    def test_app_creation(self):
        """Test that the app is created correctly."""
        assert isinstance(app, Starlette)
        assert app.debug is True
        # The routes are the same structure, just different object instances
        assert len(app.routes) == len(routes)

    def test_app_routes_structure(self):
        """Test that routes are properly configured."""
        # Should have homepage route
        homepage_route = None
        sse_route = None
        messages_mount = None
        
        for route in routes:
            if hasattr(route, 'path') and route.path == "/":
                homepage_route = route
            elif hasattr(route, 'path') and route.path == "/sse":
                sse_route = route
            elif hasattr(route, 'path') and route.path.startswith('/messages'):
                messages_mount = route
        
        assert homepage_route is not None
        assert sse_route is not None
        assert messages_mount is not None


class TestHomepage:
    """Test cases for the homepage endpoint."""

    @pytest.mark.asyncio
    async def test_homepage_response(self):
        """Test homepage returns correct response."""
        # Create a mock request
        class MockRequest:
            pass
        
        request = MockRequest()
        response = await homepage(request)
        
        assert response.status_code == 200
        assert "text/html" in response.media_type
        assert "Itinerary planning MCP server" in response.body.decode()


class TestRoutes:
    """Integration tests for the application routes."""

    def test_homepage_route(self):
        """Test the homepage route via test client."""
        client = TestClient(app)
        response = client.get("/")
        
        assert response.status_code == 200
        assert "Itinerary planning MCP server" in response.text

    def test_nonexistent_route(self):
        """Test that nonexistent routes return 404."""
        client = TestClient(app)
        response = client.get("/nonexistent")
        
        assert response.status_code == 404


class TestSSEHandling:
    """Test cases for SSE (Server-Sent Events) handling."""

    @pytest.mark.asyncio
    async def test_handle_sse_function_exists(self):
        """Test that handle_sse function is properly defined."""
        assert callable(handle_sse)
        
    # Note: Testing SSE functionality fully would require more complex mocking
    # of the MCP server transport layer, which is beyond the scope of basic unit tests