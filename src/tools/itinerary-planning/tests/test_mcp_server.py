import pytest
import re
from datetime import datetime, date
from unittest.mock import patch, MagicMock
from src.mcp_server import suggest_hotels, suggest_flights, validate_iso_date


class TestValidateIsoDate:
    """Tests for the validate_iso_date function."""

    def test_validate_iso_date_valid_format(self):
        """Test that valid ISO date strings are parsed correctly."""
        test_date = "2024-03-15"
        result = validate_iso_date(test_date, "test_date")
        expected = datetime(2024, 3, 15).date()
        assert result == expected

    def test_validate_iso_date_invalid_format(self):
        """Test that invalid date formats raise ValueError."""
        with pytest.raises(ValueError, match="test_date must be in ISO format"):
            validate_iso_date("15-03-2024", "test_date")

        with pytest.raises(ValueError, match="test_date must be in ISO format"):
            validate_iso_date("2024/03/15", "test_date")

        with pytest.raises(ValueError, match="test_date must be in ISO format"):
            validate_iso_date("03-15-2024", "test_date")

    def test_validate_iso_date_invalid_date(self):
        """Test that invalid dates raise ValueError."""
        with pytest.raises(ValueError, match="Invalid test_date"):
            validate_iso_date("2024-13-01", "test_date")

        with pytest.raises(ValueError, match="Invalid test_date"):
            validate_iso_date("2024-02-30", "test_date")

    def test_validate_iso_date_empty_string(self):
        """Test that empty string raises ValueError."""
        with pytest.raises(ValueError, match="test_date must be in ISO format"):
            validate_iso_date("", "test_date")


@pytest.mark.asyncio
class TestSuggestHotels:
    """Tests for the suggest_hotels function."""

    async def test_suggest_hotels_valid_input(self):
        """Test suggest_hotels with valid inputs."""
        location = "Paris"
        check_in = "2024-06-01"
        check_out = "2024-06-05"

        result = await suggest_hotels(location, check_in, check_out)

        assert isinstance(result, list)
        assert 3 <= len(result) <= 8
        
        # Check first hotel structure
        hotel = result[0]
        assert "name" in hotel
        assert "address" in hotel
        assert "location" in hotel
        assert "rating" in hotel
        assert "price_per_night" in hotel
        assert "hotel_type" in hotel
        assert "amenities" in hotel
        assert "available_rooms" in hotel

        # Validate data types and ranges
        assert isinstance(hotel["rating"], float)
        assert 3.0 <= hotel["rating"] <= 5.0
        assert isinstance(hotel["price_per_night"], (int, float))
        assert hotel["price_per_night"] > 0
        assert isinstance(hotel["available_rooms"], int)
        assert hotel["available_rooms"] >= 1
        assert location in hotel["location"]

    async def test_suggest_hotels_invalid_dates(self):
        """Test suggest_hotels with invalid date formats."""
        with pytest.raises(ValueError, match="check_in must be in ISO format"):
            await suggest_hotels("Paris", "01-06-2024", "2024-06-05")

        with pytest.raises(ValueError, match="check_out must be in ISO format"):
            await suggest_hotels("Paris", "2024-06-01", "05-06-2024")

    async def test_suggest_hotels_checkout_before_checkin(self):
        """Test suggest_hotels when checkout is before or same as checkin."""
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels("Paris", "2024-06-05", "2024-06-01")

        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels("Paris", "2024-06-05", "2024-06-05")

    async def test_suggest_hotels_hotel_types_present(self):
        """Test that hotels include valid types."""
        result = await suggest_hotels("Tokyo", "2024-07-01", "2024-07-03")
        valid_types = ["Luxury", "Boutique", "Budget", "Business"]
        
        for hotel in result:
            assert hotel["hotel_type"] in valid_types

    async def test_suggest_hotels_amenities_structure(self):
        """Test that hotels have proper amenities."""
        result = await suggest_hotels("London", "2024-08-01", "2024-08-03")
        valid_amenities = ["Free WiFi", "Pool", "Spa", "Gym", "Restaurant", "Bar", "Room Service", "Parking"]
        
        for hotel in result:
            assert isinstance(hotel["amenities"], list)
            assert 3 <= len(hotel["amenities"]) <= 6
            for amenity in hotel["amenities"]:
                assert amenity in valid_amenities

    async def test_suggest_hotels_sorted_by_rating(self):
        """Test that hotels are sorted by rating in descending order."""
        result = await suggest_hotels("Rome", "2024-09-01", "2024-09-03")
        
        ratings = [hotel["rating"] for hotel in result]
        assert ratings == sorted(ratings, reverse=True)


@pytest.mark.asyncio  
class TestSuggestFlights:
    """Tests for the suggest_flights function."""

    async def test_suggest_flights_one_way_valid(self):
        """Test suggest_flights for one-way trip with valid inputs."""
        from_location = "New York"
        to_location = "Los Angeles"
        departure_date = "2024-06-15"

        result = await suggest_flights(from_location, to_location, departure_date)

        assert isinstance(result, dict)
        assert "departure_flights" in result
        assert "return_flights" in result
        assert isinstance(result["departure_flights"], list)
        assert isinstance(result["return_flights"], list)
        assert 3 <= len(result["departure_flights"]) <= 7
        assert len(result["return_flights"]) == 0

        # Check flight structure
        flight = result["departure_flights"][0]
        required_fields = [
            "flight_id", "airline", "flight_number", "aircraft",
            "from_airport", "to_airport", "departure", "arrival",
            "duration_minutes", "is_direct", "price", "currency",
            "available_seats", "cabin_class"
        ]
        for field in required_fields:
            assert field in flight

    async def test_suggest_flights_round_trip_valid(self):
        """Test suggest_flights for round trip with valid inputs."""
        from_location = "Boston"
        to_location = "Miami"
        departure_date = "2024-07-10"
        return_date = "2024-07-17"

        result = await suggest_flights(from_location, to_location, departure_date, return_date)

        assert isinstance(result, dict)
        assert "departure_flights" in result
        assert "return_flights" in result
        assert 3 <= len(result["departure_flights"]) <= 7
        assert 3 <= len(result["return_flights"]) <= 7

        # Check return flight structure
        return_flight = result["return_flights"][0]
        assert return_flight["from_airport"]["city"] == to_location
        assert return_flight["to_airport"]["city"] == from_location

    async def test_suggest_flights_invalid_dates(self):
        """Test suggest_flights with invalid date formats."""
        with pytest.raises(ValueError, match="departure_date must be in ISO format"):
            await suggest_flights("NYC", "LA", "15-06-2024")

        with pytest.raises(ValueError, match="return_date must be in ISO format"):
            await suggest_flights("NYC", "LA", "2024-06-15", "17-06-2024")

    async def test_suggest_flights_return_before_departure(self):
        """Test suggest_flights when return is before or same as departure."""
        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights("NYC", "LA", "2024-06-15", "2024-06-10")

        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights("NYC", "LA", "2024-06-15", "2024-06-15")

    async def test_suggest_flights_airport_codes_generated(self):
        """Test that airport codes are generated for cities."""
        result = await suggest_flights("Seattle", "Denver", "2024-08-01")
        
        departure_flight = result["departure_flights"][0]
        from_code = departure_flight["from_airport"]["code"]
        to_code = departure_flight["to_airport"]["code"]
        
        # Airport codes should be 3 characters
        assert len(from_code) == 3
        assert len(to_code) == 3
        assert from_code.isupper()
        assert to_code.isupper()

    async def test_suggest_flights_price_and_duration_valid(self):
        """Test that flight prices and durations are within expected ranges."""
        result = await suggest_flights("Chicago", "Phoenix", "2024-09-01")
        
        for flight in result["departure_flights"]:
            assert isinstance(flight["price"], (int, float))
            assert 99 <= flight["price"] <= 999
            assert flight["currency"] == "USD"
            assert isinstance(flight["duration_minutes"], int)
            assert 60 <= flight["duration_minutes"] <= 480
            assert isinstance(flight["available_seats"], int)
            assert 1 <= flight["available_seats"] <= 30

    async def test_suggest_flights_cabin_classes_valid(self):
        """Test that flights have valid cabin classes."""
        result = await suggest_flights("Atlanta", "Dallas", "2024-10-01")
        valid_classes = ["Economy", "Premium Economy", "Business", "First"]
        
        for flight in result["departure_flights"]:
            assert flight["cabin_class"] in valid_classes

    async def test_suggest_flights_connection_structure(self):
        """Test connecting flights have proper structure."""
        result = await suggest_flights("Portland", "Orlando", "2024-11-01")
        
        for flight in result["departure_flights"]:
            if not flight["is_direct"]:
                assert "segments" in flight
                assert "connection_airport" in flight
                assert "connection_duration_minutes" in flight
                assert len(flight["segments"]) == 2
                
                # Check segment structure
                for segment in flight["segments"]:
                    assert "flight_number" in segment
                    assert "from_airport" in segment
                    assert "to_airport" in segment
                    assert "departure" in segment
                    assert "arrival" in segment
                    assert "duration_minutes" in segment

    async def test_suggest_flights_datetime_format(self):
        """Test that flight times are in proper ISO format."""
        result = await suggest_flights("Las Vegas", "Nashville", "2024-12-01")
        
        flight = result["departure_flights"][0]
        
        # Test datetime parsing
        departure_time = datetime.fromisoformat(flight["departure"])
        arrival_time = datetime.fromisoformat(flight["arrival"])
        
        assert arrival_time > departure_time
        assert departure_time.date() == datetime(2024, 12, 1).date()