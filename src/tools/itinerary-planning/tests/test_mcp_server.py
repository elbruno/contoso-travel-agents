"""
Tests for the MCP server tools (suggest_hotels and suggest_flights).
"""
import pytest
from datetime import datetime, date
from unittest.mock import patch
import sys
import os

# Add src directory to path so we can import the modules
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from mcp_server import suggest_hotels, suggest_flights, validate_iso_date


class TestValidateIsoDate:
    """Tests for the validate_iso_date function."""
    
    def test_valid_iso_date(self):
        """Test that valid ISO dates are parsed correctly."""
        result = validate_iso_date("2024-12-25", "test_param")
        expected = date(2024, 12, 25)
        assert result == expected
    
    def test_invalid_format(self):
        """Test that invalid date formats raise ValueError."""
        with pytest.raises(ValueError, match="test_param must be in ISO format"):
            validate_iso_date("25-12-2024", "test_param")
    
    def test_invalid_date(self):
        """Test that invalid dates raise ValueError."""
        with pytest.raises(ValueError, match="Invalid test_param"):
            validate_iso_date("2024-13-01", "test_param")
    
    def test_february_29_leap_year(self):
        """Test that leap year February 29 is valid."""
        result = validate_iso_date("2024-02-29", "test_param")
        expected = date(2024, 2, 29)
        assert result == expected
    
    def test_february_29_non_leap_year(self):
        """Test that non-leap year February 29 is invalid."""
        with pytest.raises(ValueError, match="Invalid test_param"):
            validate_iso_date("2023-02-29", "test_param")


class TestSuggestHotels:
    """Tests for the suggest_hotels function."""
    
    @pytest.mark.asyncio
    async def test_valid_hotel_search(self):
        """Test that valid hotel search returns expected structure."""
        result = await suggest_hotels(
            location="Paris",
            check_in="2024-12-01",
            check_out="2024-12-05"
        )
        
        assert isinstance(result, list)
        assert len(result) >= 3  # Should return at least 3 hotels
        assert len(result) <= 8  # Should return at most 8 hotels
        
        # Check structure of first hotel
        hotel = result[0]
        required_fields = ["name", "address", "location", "rating", "price_per_night", 
                          "hotel_type", "amenities", "available_rooms"]
        for field in required_fields:
            assert field in hotel
        
        # Check data types and ranges
        assert isinstance(hotel["rating"], float)
        assert 3.0 <= hotel["rating"] <= 5.0
        assert isinstance(hotel["price_per_night"], (int, float))
        assert hotel["price_per_night"] > 0
        assert isinstance(hotel["amenities"], list)
        assert len(hotel["amenities"]) >= 3
        assert isinstance(hotel["available_rooms"], int)
        assert hotel["available_rooms"] >= 1
    
    @pytest.mark.asyncio
    async def test_hotels_sorted_by_rating(self):
        """Test that hotels are sorted by rating in descending order."""
        result = await suggest_hotels(
            location="London",
            check_in="2024-12-01",
            check_out="2024-12-05"
        )
        
        ratings = [hotel["rating"] for hotel in result]
        assert ratings == sorted(ratings, reverse=True)
    
    @pytest.mark.asyncio
    async def test_invalid_checkout_before_checkin(self):
        """Test that check_out before check_in raises ValueError."""
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(
                location="Paris",
                check_in="2024-12-05",
                check_out="2024-12-01"
            )
    
    @pytest.mark.asyncio
    async def test_same_checkin_checkout_dates(self):
        """Test that same check_in and check_out dates raise ValueError."""
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(
                location="Paris",
                check_in="2024-12-01",
                check_out="2024-12-01"
            )
    
    @pytest.mark.asyncio
    async def test_invalid_checkin_date_format(self):
        """Test that invalid check_in format raises ValueError."""
        with pytest.raises(ValueError, match="check_in must be in ISO format"):
            await suggest_hotels(
                location="Paris",
                check_in="01-12-2024",
                check_out="2024-12-05"
            )
    
    @pytest.mark.asyncio
    async def test_hotel_types_are_valid(self):
        """Test that all hotel types are from expected list."""
        valid_types = ["Luxury", "Boutique", "Budget", "Business"]
        
        result = await suggest_hotels(
            location="Tokyo",
            check_in="2024-12-01",
            check_out="2024-12-05"
        )
        
        for hotel in result:
            assert hotel["hotel_type"] in valid_types


class TestSuggestFlights:
    """Tests for the suggest_flights function."""
    
    @pytest.mark.asyncio
    async def test_one_way_flight_search(self):
        """Test one-way flight search returns correct structure."""
        result = await suggest_flights(
            from_location="New York",
            to_location="London",
            departure_date="2024-12-01"
        )
        
        assert isinstance(result, dict)
        assert "departure_flights" in result
        assert "return_flights" in result
        assert isinstance(result["departure_flights"], list)
        assert isinstance(result["return_flights"], list)
        assert len(result["return_flights"]) == 0  # No return flights for one-way
        
        # Check departure flights structure
        assert len(result["departure_flights"]) >= 3
        flight = result["departure_flights"][0]
        
        required_fields = ["flight_id", "airline", "flight_number", "aircraft",
                          "from_airport", "to_airport", "departure", "arrival",
                          "duration_minutes", "is_direct", "price", "currency",
                          "available_seats", "cabin_class"]
        for field in required_fields:
            assert field in flight
        
        # Check airport structure
        assert "code" in flight["from_airport"]
        assert "name" in flight["from_airport"]
        assert "city" in flight["from_airport"]
    
    @pytest.mark.asyncio
    async def test_round_trip_flight_search(self):
        """Test round-trip flight search returns both directions."""
        result = await suggest_flights(
            from_location="Paris",
            to_location="Tokyo",
            departure_date="2024-12-01",
            return_date="2024-12-15"
        )
        
        assert len(result["departure_flights"]) >= 3
        assert len(result["return_flights"]) >= 3
        
        # Check that return flights go in opposite direction
        dep_flight = result["departure_flights"][0]
        ret_flight = result["return_flights"][0]
        
        assert dep_flight["from_airport"]["city"] == ret_flight["to_airport"]["city"]
        assert dep_flight["to_airport"]["city"] == ret_flight["from_airport"]["city"]
    
    @pytest.mark.asyncio
    async def test_invalid_return_before_departure(self):
        """Test that return date before departure raises ValueError."""
        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights(
                from_location="New York",
                to_location="London",
                departure_date="2024-12-15",
                return_date="2024-12-01"
            )
    
    @pytest.mark.asyncio
    async def test_invalid_departure_date_format(self):
        """Test that invalid departure_date format raises ValueError."""
        with pytest.raises(ValueError, match="departure_date must be in ISO format"):
            await suggest_flights(
                from_location="New York",
                to_location="London",
                departure_date="01-12-2024"
            )
    
    @pytest.mark.asyncio
    async def test_flight_prices_are_reasonable(self):
        """Test that flight prices are within reasonable range."""
        result = await suggest_flights(
            from_location="Chicago",
            to_location="Miami",
            departure_date="2024-12-01"
        )
        
        for flight in result["departure_flights"]:
            assert 99 <= flight["price"] <= 999
            assert flight["currency"] == "USD"
    
    @pytest.mark.asyncio
    async def test_flight_times_are_realistic(self):
        """Test that flight times are realistic."""
        result = await suggest_flights(
            from_location="Boston",
            to_location="Seattle",
            departure_date="2024-12-01"
        )
        
        for flight in result["departure_flights"]:
            # Flight duration should be between 1 and 8 hours
            assert 60 <= flight["duration_minutes"] <= 480
            
            # Departure should be between 6 AM and 10 PM
            dep_time = datetime.fromisoformat(flight["departure"])
            assert 6 <= dep_time.hour <= 22
    
    @pytest.mark.asyncio
    async def test_connecting_flights_have_segments(self):
        """Test that non-direct flights have segment information."""
        # Run multiple times to increase chance of getting a connecting flight
        for _ in range(5):
            result = await suggest_flights(
                from_location="Denver",
                to_location="Frankfurt",
                departure_date="2024-12-01"
            )
            
            for flight in result["departure_flights"]:
                if not flight["is_direct"]:
                    assert "segments" in flight
                    assert "connection_airport" in flight
                    assert "connection_duration_minutes" in flight
                    assert len(flight["segments"]) == 2
                    assert 45 <= flight["connection_duration_minutes"] <= 180
                    return  # Found a connecting flight, test passed
    
    @pytest.mark.asyncio
    async def test_cabin_classes_are_valid(self):
        """Test that cabin classes are from expected list."""
        valid_classes = ["Economy", "Premium Economy", "Business", "First"]
        
        result = await suggest_flights(
            from_location="Los Angeles",
            to_location="New York",
            departure_date="2024-12-01"
        )
        
        for flight in result["departure_flights"]:
            assert flight["cabin_class"] in valid_classes


@pytest.mark.asyncio
async def test_flight_search_reproducibility():
    """Test that flight searches with same parameters produce consistent structure."""
    # Note: We don't test for identical results since the function uses randomization
    # But we ensure the structure and data ranges are consistent
    
    result1 = await suggest_flights(
        from_location="Dallas",
        to_location="Phoenix",
        departure_date="2024-12-01"
    )
    
    result2 = await suggest_flights(
        from_location="Dallas",
        to_location="Phoenix",
        departure_date="2024-12-01"
    )
    
    # Both should have similar structure
    assert len(result1["departure_flights"]) >= 3
    assert len(result2["departure_flights"]) >= 3
    assert len(result1["return_flights"]) == 0
    assert len(result2["return_flights"]) == 0


@pytest.mark.asyncio
async def test_hotel_search_reproducibility():
    """Test that hotel searches with same parameters produce consistent structure."""
    result1 = await suggest_hotels(
        location="Miami",
        check_in="2024-12-01",
        check_out="2024-12-05"
    )
    
    result2 = await suggest_hotels(
        location="Miami",
        check_in="2024-12-01",
        check_out="2024-12-05"
    )
    
    # Both should have similar structure and count
    assert len(result1) >= 3
    assert len(result2) >= 3
    assert len(result1) <= 8
    assert len(result2) <= 8