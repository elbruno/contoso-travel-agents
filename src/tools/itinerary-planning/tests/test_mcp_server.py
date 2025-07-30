import pytest
import re
from datetime import datetime, timedelta
import sys
import os

# Add the src directory to the Python path so we can import our modules
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from mcp_server import suggest_hotels, suggest_flights, validate_iso_date


class TestValidateIsoDate:
    """Test cases for the validate_iso_date function"""
    
    def test_valid_iso_date(self):
        """Test that valid ISO dates are parsed correctly"""
        date_str = "2024-12-25"
        result = validate_iso_date(date_str, "test_date")
        assert result.year == 2024
        assert result.month == 12
        assert result.day == 25
    
    def test_invalid_format_raises_error(self):
        """Test that invalid date formats raise ValueError"""
        invalid_dates = [
            "25-12-2024",  # Wrong format
            "2024/12/25",  # Wrong separator
            "2024-12-25T10:30:00",  # With time
            "Dec 25, 2024",  # Named month
            "2024-13-01",  # Invalid month
            "2024-12-32",  # Invalid day
        ]
        
        for invalid_date in invalid_dates:
            with pytest.raises(ValueError):
                validate_iso_date(invalid_date, "test_date")
    
    def test_empty_string_raises_error(self):
        """Test that empty string raises ValueError"""
        with pytest.raises(ValueError):
            validate_iso_date("", "test_date")
    
    def test_none_raises_error(self):
        """Test that None raises an error"""
        with pytest.raises(TypeError):
            validate_iso_date(None, "test_date")


class TestSuggestHotels:
    """Test cases for the suggest_hotels function"""
    
    @pytest.mark.asyncio
    async def test_valid_hotel_suggestion(self):
        """Test that valid hotel suggestions are returned"""
        location = "New York"
        check_in = "2024-12-25"
        check_out = "2024-12-30"
        
        result = await suggest_hotels(location, check_in, check_out)
        
        # Check that we get a list of hotels
        assert isinstance(result, list)
        assert len(result) >= 3  # Should get at least 3 hotels
        assert len(result) <= 8  # Should get at most 8 hotels
        
        # Check structure of first hotel
        hotel = result[0]
        required_fields = ["name", "address", "location", "rating", "price_per_night", 
                          "hotel_type", "amenities", "available_rooms"]
        
        for field in required_fields:
            assert field in hotel
        
        # Check data types and ranges
        assert isinstance(hotel["name"], str)
        assert isinstance(hotel["address"], str)
        assert isinstance(hotel["location"], str)
        assert location in hotel["location"]
        assert isinstance(hotel["rating"], float)
        assert 3.0 <= hotel["rating"] <= 5.0
        assert isinstance(hotel["price_per_night"], int)
        assert hotel["price_per_night"] > 0
        assert hotel["hotel_type"] in ["Luxury", "Boutique", "Budget", "Business"]
        assert isinstance(hotel["amenities"], list)
        assert len(hotel["amenities"]) >= 3
        assert isinstance(hotel["available_rooms"], int)
        assert hotel["available_rooms"] >= 1
    
    @pytest.mark.asyncio
    async def test_invalid_dates_raise_error(self):
        """Test that invalid dates raise appropriate errors"""
        location = "Paris"
        
        # Check-out before check-in
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(location, "2024-12-30", "2024-12-25")
        
        # Same dates
        with pytest.raises(ValueError, match="check_out date must be after check_in date"):
            await suggest_hotels(location, "2024-12-25", "2024-12-25")
        
        # Invalid date format
        with pytest.raises(ValueError):
            await suggest_hotels(location, "25-12-2024", "30-12-2024")
    
    @pytest.mark.asyncio
    async def test_hotels_sorted_by_rating(self):
        """Test that hotels are sorted by rating in descending order"""
        location = "London"
        check_in = "2024-12-25"
        check_out = "2024-12-30"
        
        result = await suggest_hotels(location, check_in, check_out)
        
        # Check that ratings are sorted in descending order
        ratings = [hotel["rating"] for hotel in result]
        assert ratings == sorted(ratings, reverse=True)
    
    @pytest.mark.asyncio
    async def test_different_locations_work(self):
        """Test that different locations work correctly"""
        check_in = "2024-12-25"
        check_out = "2024-12-30"
        
        locations = ["Tokyo", "Sydney", "Berlin", "São Paulo"]
        
        for location in locations:
            result = await suggest_hotels(location, check_in, check_out)
            assert isinstance(result, list)
            assert len(result) > 0
            # Check that location appears in hotel location
            assert any(location in hotel["location"] for hotel in result)


class TestSuggestFlights:
    """Test cases for the suggest_flights function"""
    
    @pytest.mark.asyncio
    async def test_one_way_flight_suggestion(self):
        """Test one-way flight suggestions"""
        from_location = "New York"
        to_location = "London"
        departure_date = "2024-12-25"
        
        result = await suggest_flights(from_location, to_location, departure_date)
        
        assert isinstance(result, dict)
        assert "departure_flights" in result
        assert "return_flights" in result
        assert isinstance(result["departure_flights"], list)
        assert isinstance(result["return_flights"], list)
        assert len(result["departure_flights"]) >= 3
        assert len(result["return_flights"]) == 0  # No return flights for one-way
        
        # Check structure of first flight
        flight = result["departure_flights"][0]
        required_fields = ["flight_id", "airline", "flight_number", "aircraft", 
                          "from_airport", "to_airport", "departure", "arrival", 
                          "duration_minutes", "is_direct", "price", "currency", 
                          "available_seats", "cabin_class"]
        
        for field in required_fields:
            assert field in flight
        
        # Check data types and values
        assert isinstance(flight["flight_id"], str)
        assert len(flight["flight_id"]) == 8
        assert isinstance(flight["airline"], str)
        assert isinstance(flight["flight_number"], str)
        assert isinstance(flight["aircraft"], str)
        assert isinstance(flight["from_airport"], dict)
        assert isinstance(flight["to_airport"], dict)
        assert isinstance(flight["departure"], str)
        assert isinstance(flight["arrival"], str)
        assert isinstance(flight["duration_minutes"], int)
        assert 60 <= flight["duration_minutes"] <= 480
        assert isinstance(flight["is_direct"], bool)
        assert isinstance(flight["price"], float)
        assert flight["price"] > 0
        assert flight["currency"] == "USD"
        assert isinstance(flight["available_seats"], int)
        assert flight["available_seats"] >= 1
        assert flight["cabin_class"] in ["Economy", "Premium Economy", "Business", "First"]
    
    @pytest.mark.asyncio
    async def test_round_trip_flight_suggestion(self):
        """Test round-trip flight suggestions"""
        from_location = "Paris"
        to_location = "Tokyo"
        departure_date = "2024-12-25"
        return_date = "2025-01-05"
        
        result = await suggest_flights(from_location, to_location, departure_date, return_date)
        
        assert isinstance(result, dict)
        assert "departure_flights" in result
        assert "return_flights" in result
        assert len(result["departure_flights"]) >= 3
        assert len(result["return_flights"]) >= 3  # Should have return flights
        
        # Check that return flights go in opposite direction
        dep_flight = result["departure_flights"][0]
        ret_flight = result["return_flights"][0]
        
        assert dep_flight["from_airport"]["city"] == from_location
        assert dep_flight["to_airport"]["city"] == to_location
        assert ret_flight["from_airport"]["city"] == to_location
        assert ret_flight["to_airport"]["city"] == from_location
    
    @pytest.mark.asyncio
    async def test_invalid_flight_dates_raise_error(self):
        """Test that invalid flight dates raise appropriate errors"""
        from_location = "Berlin"
        to_location = "Sydney"
        
        # Return date before departure date
        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights(from_location, to_location, "2024-12-30", "2024-12-25")
        
        # Same dates
        with pytest.raises(ValueError, match="return_date must be after departure_date"):
            await suggest_flights(from_location, to_location, "2024-12-25", "2024-12-25")
        
        # Invalid date format
        with pytest.raises(ValueError):
            await suggest_flights(from_location, to_location, "25-12-2024")
    
    @pytest.mark.asyncio
    async def test_connecting_flights_have_segments(self):
        """Test that connecting flights have proper segment information"""
        from_location = "Seattle"
        to_location = "Mumbai"
        departure_date = "2024-12-25"
        
        result = await suggest_flights(from_location, to_location, departure_date)
        
        # Find a connecting flight (if any)
        connecting_flights = [f for f in result["departure_flights"] if not f["is_direct"]]
        
        if connecting_flights:
            flight = connecting_flights[0]
            assert "segments" in flight
            assert "connection_airport" in flight
            assert "connection_duration_minutes" in flight
            assert isinstance(flight["segments"], list)
            assert len(flight["segments"]) == 2
            
            # Check segment structure
            for segment in flight["segments"]:
                assert "flight_number" in segment
                assert "from_airport" in segment
                assert "to_airport" in segment
                assert "departure" in segment
                assert "arrival" in segment
                assert "duration_minutes" in segment
    
    @pytest.mark.asyncio
    async def test_datetime_parsing_in_flights(self):
        """Test that flight departure and arrival times are valid ISO format"""
        from_location = "Amsterdam"
        to_location = "Bangkok"
        departure_date = "2024-12-25"
        
        result = await suggest_flights(from_location, to_location, departure_date)
        
        for flight in result["departure_flights"]:
            # Test that departure and arrival can be parsed as datetime
            dep_time = datetime.fromisoformat(flight["departure"])
            arr_time = datetime.fromisoformat(flight["arrival"])
            
            # Arrival should be after departure
            assert arr_time > dep_time
            
            # Departure should be on the specified date
            assert dep_time.date() == datetime.strptime(departure_date, "%Y-%m-%d").date()
    
    @pytest.mark.asyncio
    async def test_flight_times_reasonable(self):
        """Test that flight departure times are within reasonable hours"""
        from_location = "Chicago"
        to_location = "Rome"
        departure_date = "2024-12-25"
        
        result = await suggest_flights(from_location, to_location, departure_date)
        
        for flight in result["departure_flights"]:
            dep_time = datetime.fromisoformat(flight["departure"])
            # Flights should depart between 6 AM and 10 PM
            assert 6 <= dep_time.hour <= 22
            # Minutes should be in 15-minute intervals
            assert dep_time.minute in [0, 15, 30, 45]
    
    @pytest.mark.asyncio
    async def test_airport_codes_format(self):
        """Test that airport codes are properly formatted"""
        from_location = "Vancouver"
        to_location = "Dubai"
        departure_date = "2024-12-25"
        
        result = await suggest_flights(from_location, to_location, departure_date)
        
        flight = result["departure_flights"][0]
        
        # Airport codes should be 3 characters and uppercase
        from_code = flight["from_airport"]["code"]
        to_code = flight["to_airport"]["code"]
        
        assert isinstance(from_code, str)
        assert isinstance(to_code, str)
        assert len(from_code) == 3
        assert len(to_code) == 3
        assert from_code.isupper()
        assert to_code.isupper()
        assert from_code.isalpha()
        assert to_code.isalpha()


class TestIntegration:
    """Integration tests for the MCP server functionality"""
    
    @pytest.mark.asyncio
    async def test_complete_travel_planning_scenario(self):
        """Test a complete travel planning scenario with hotels and flights"""
        location = "Barcelona"
        from_location = "London"
        departure_date = "2024-12-25"
        return_date = "2024-12-30"
        
        # Get hotel suggestions
        hotels = await suggest_hotels(location, departure_date, return_date)
        
        # Get flight suggestions
        flights = await suggest_flights(from_location, location, departure_date, return_date)
        
        # Verify we have both hotels and flights
        assert len(hotels) > 0
        assert len(flights["departure_flights"]) > 0
        assert len(flights["return_flights"]) > 0
        
        # Verify the dates align
        # Hotels should be available for the stay duration
        check_in_date = datetime.strptime(departure_date, "%Y-%m-%d").date()
        check_out_date = datetime.strptime(return_date, "%Y-%m-%d").date()
        
        # Flights should be on the right dates
        dep_flight = flights["departure_flights"][0]
        ret_flight = flights["return_flights"][0]
        
        dep_flight_date = datetime.fromisoformat(dep_flight["departure"]).date()
        ret_flight_date = datetime.fromisoformat(ret_flight["departure"]).date()
        
        assert dep_flight_date == check_in_date
        assert ret_flight_date == check_out_date
        
        # Verify destinations match
        assert location in hotels[0]["location"]
        assert dep_flight["to_airport"]["city"] == location
        assert ret_flight["from_airport"]["city"] == location