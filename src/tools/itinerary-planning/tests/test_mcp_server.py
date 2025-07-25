import unittest
import asyncio
import sys
import os
from datetime import datetime, timedelta

# Add src directory to path to import modules
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'src'))

from mcp_server import suggest_hotels, suggest_flights, validate_iso_date


class TestMCPServerFunctions(unittest.TestCase):
    """Test suite for MCP server functions."""

    def setUp(self):
        """Set up test fixtures."""
        self.loop = asyncio.new_event_loop()
        asyncio.set_event_loop(self.loop)

    def tearDown(self):
        """Clean up after tests."""
        self.loop.close()

    def test_validate_iso_date_valid_date(self):
        """Test validate_iso_date with valid date string."""
        result = validate_iso_date("2024-03-15", "test_date")
        self.assertEqual(result, datetime(2024, 3, 15).date())

    def test_validate_iso_date_invalid_format(self):
        """Test validate_iso_date with invalid format."""
        with self.assertRaises(ValueError) as cm:
            validate_iso_date("15-03-2024", "test_date")
        self.assertIn("must be in ISO format", str(cm.exception))

    def test_validate_iso_date_invalid_date(self):
        """Test validate_iso_date with invalid date."""
        with self.assertRaises(ValueError) as cm:
            validate_iso_date("2024-13-45", "test_date")
        self.assertIn("Invalid test_date", str(cm.exception))

    def test_suggest_hotels_valid_input(self):
        """Test suggest_hotels with valid input."""
        async def run_test():
            result = await suggest_hotels("New York", "2024-03-15", "2024-03-17")
            
            # Check that result is a list
            self.assertIsInstance(result, list)
            
            # Check that we have between 3 and 8 hotels
            self.assertGreaterEqual(len(result), 3)
            self.assertLessEqual(len(result), 8)
            
            # Check structure of first hotel
            if result:
                hotel = result[0]
                required_fields = ['name', 'address', 'location', 'rating', 'price_per_night', 
                                 'hotel_type', 'amenities', 'available_rooms']
                for field in required_fields:
                    self.assertIn(field, hotel)
                
                # Check data types
                self.assertIsInstance(hotel['rating'], float)
                self.assertGreaterEqual(hotel['rating'], 3.0)
                self.assertLessEqual(hotel['rating'], 5.0)
                
                self.assertIsInstance(hotel['price_per_night'], (int, float))
                self.assertGreater(hotel['price_per_night'], 0)
                
                self.assertIsInstance(hotel['amenities'], list)
                self.assertGreater(len(hotel['amenities']), 0)
                
                self.assertIsInstance(hotel['available_rooms'], int)
                self.assertGreater(hotel['available_rooms'], 0)

        self.loop.run_until_complete(run_test())

    def test_suggest_hotels_invalid_date_format(self):
        """Test suggest_hotels with invalid date format."""
        async def run_test():
            with self.assertRaises(ValueError):
                await suggest_hotels("New York", "15-03-2024", "17-03-2024")

        self.loop.run_until_complete(run_test())

    def test_suggest_hotels_checkout_before_checkin(self):
        """Test suggest_hotels with checkout date before checkin."""
        async def run_test():
            with self.assertRaises(ValueError) as cm:
                await suggest_hotels("New York", "2024-03-17", "2024-03-15")
            self.assertIn("check_out date must be after check_in date", str(cm.exception))

        self.loop.run_until_complete(run_test())

    def test_suggest_flights_valid_input_oneway(self):
        """Test suggest_flights with valid one-way input."""
        async def run_test():
            result = await suggest_flights("New York", "Paris", "2024-03-15")
            
            # Check that result is a dict with expected keys
            self.assertIsInstance(result, dict)
            self.assertIn('departure_flights', result)
            self.assertIn('return_flights', result)
            
            # Check departure flights
            departure_flights = result['departure_flights']
            self.assertIsInstance(departure_flights, list)
            self.assertGreaterEqual(len(departure_flights), 3)
            self.assertLessEqual(len(departure_flights), 7)
            
            # Check return flights should be empty for one-way
            return_flights = result['return_flights']
            self.assertIsInstance(return_flights, list)
            self.assertEqual(len(return_flights), 0)
            
            # Check structure of first flight
            if departure_flights:
                flight = departure_flights[0]
                required_fields = ['flight_id', 'airline', 'flight_number', 'aircraft',
                                 'from_airport', 'to_airport', 'departure', 'arrival',
                                 'duration_minutes', 'is_direct', 'price', 'currency',
                                 'available_seats', 'cabin_class']
                for field in required_fields:
                    self.assertIn(field, flight)
                
                # Check data types
                self.assertIsInstance(flight['duration_minutes'], int)
                self.assertGreaterEqual(flight['duration_minutes'], 60)
                self.assertLessEqual(flight['duration_minutes'], 480)
                
                self.assertIsInstance(flight['price'], float)
                self.assertGreater(flight['price'], 0)
                
                self.assertIsInstance(flight['available_seats'], int)
                self.assertGreater(flight['available_seats'], 0)
                
                self.assertIsInstance(flight['is_direct'], bool)

        self.loop.run_until_complete(run_test())

    def test_suggest_flights_valid_input_roundtrip(self):
        """Test suggest_flights with valid round-trip input."""
        async def run_test():
            result = await suggest_flights("New York", "Paris", "2024-03-15", "2024-03-22")
            
            # Check that result is a dict with expected keys
            self.assertIsInstance(result, dict)
            self.assertIn('departure_flights', result)
            self.assertIn('return_flights', result)
            
            # Check departure flights
            departure_flights = result['departure_flights']
            self.assertIsInstance(departure_flights, list)
            self.assertGreaterEqual(len(departure_flights), 3)
            self.assertLessEqual(len(departure_flights), 7)
            
            # Check return flights should not be empty for round-trip
            return_flights = result['return_flights']
            self.assertIsInstance(return_flights, list)
            self.assertGreaterEqual(len(return_flights), 3)
            self.assertLessEqual(len(return_flights), 7)

        self.loop.run_until_complete(run_test())

    def test_suggest_flights_invalid_date_format(self):
        """Test suggest_flights with invalid date format."""
        async def run_test():
            with self.assertRaises(ValueError):
                await suggest_flights("New York", "Paris", "15-03-2024")

        self.loop.run_until_complete(run_test())

    def test_suggest_flights_return_before_departure(self):
        """Test suggest_flights with return date before departure."""
        async def run_test():
            with self.assertRaises(ValueError) as cm:
                await suggest_flights("New York", "Paris", "2024-03-15", "2024-03-10")
            self.assertIn("return_date must be after departure_date", str(cm.exception))

        self.loop.run_until_complete(run_test())

    def test_suggest_flights_connecting_flights_structure(self):
        """Test that connecting flights have proper structure."""
        async def run_test():
            result = await suggest_flights("New York", "Tokyo", "2024-03-15")
            departure_flights = result['departure_flights']
            
            # Find a connecting flight (40% chance, so we run a few times)
            connecting_flight = None
            for flight in departure_flights:
                if not flight['is_direct']:
                    connecting_flight = flight
                    break
            
            # If we found a connecting flight, validate its structure
            if connecting_flight:
                self.assertIn('segments', connecting_flight)
                self.assertIn('connection_airport', connecting_flight)
                self.assertIn('connection_duration_minutes', connecting_flight)
                
                segments = connecting_flight['segments']
                self.assertEqual(len(segments), 2)
                
                # Each segment should have proper structure
                for segment in segments:
                    segment_fields = ['flight_number', 'from_airport', 'to_airport',
                                    'departure', 'arrival', 'duration_minutes']
                    for field in segment_fields:
                        self.assertIn(field, segment)

        self.loop.run_until_complete(run_test())

    def test_airport_code_generation(self):
        """Test that airport codes are generated properly."""
        async def run_test():
            result = await suggest_flights("New York", "Paris", "2024-03-15")
            departure_flights = result['departure_flights']
            
            if departure_flights:
                flight = departure_flights[0]
                from_airport = flight['from_airport']
                to_airport = flight['to_airport']
                
                # Check airport structure
                airport_fields = ['code', 'name', 'city']
                for field in airport_fields:
                    self.assertIn(field, from_airport)
                    self.assertIn(field, to_airport)
                
                # Check airport code format (3 letters)
                self.assertEqual(len(from_airport['code']), 3)
                self.assertEqual(len(to_airport['code']), 3)
                self.assertTrue(from_airport['code'].isupper())
                self.assertTrue(to_airport['code'].isupper())

        self.loop.run_until_complete(run_test())


class TestDataValidation(unittest.TestCase):
    """Test suite for data validation and edge cases."""

    def setUp(self):
        """Set up test fixtures."""
        self.loop = asyncio.new_event_loop()
        asyncio.set_event_loop(self.loop)

    def tearDown(self):
        """Clean up after tests."""
        self.loop.close()

    def test_hotels_sorted_by_rating(self):
        """Test that hotels are sorted by rating in descending order."""
        async def run_test():
            result = await suggest_hotels("London", "2024-04-01", "2024-04-03")
            
            if len(result) > 1:
                for i in range(len(result) - 1):
                    self.assertGreaterEqual(result[i]['rating'], result[i + 1]['rating'])

        self.loop.run_until_complete(run_test())

    def test_hotel_types_are_valid(self):
        """Test that hotel types are from the predefined list."""
        async def run_test():
            result = await suggest_hotels("Berlin", "2024-05-01", "2024-05-03")
            valid_types = ["Luxury", "Boutique", "Budget", "Business"]
            
            for hotel in result:
                self.assertIn(hotel['hotel_type'], valid_types)

        self.loop.run_until_complete(run_test())

    def test_flight_cabin_classes_are_valid(self):
        """Test that flight cabin classes are from the predefined list."""
        async def run_test():
            result = await suggest_flights("Madrid", "Rome", "2024-06-01")
            valid_classes = ["Economy", "Premium Economy", "Business", "First"]
            
            for flight in result['departure_flights']:
                self.assertIn(flight['cabin_class'], valid_classes)

        self.loop.run_until_complete(run_test())

    def test_flight_times_are_realistic(self):
        """Test that flight departure times are within realistic hours."""
        async def run_test():
            result = await suggest_flights("Amsterdam", "Barcelona", "2024-07-01")
            
            for flight in result['departure_flights']:
                departure_time = datetime.fromisoformat(flight['departure'])
                # Flights should depart between 6 AM and 10 PM
                self.assertGreaterEqual(departure_time.hour, 6)
                self.assertLessEqual(departure_time.hour, 22)

        self.loop.run_until_complete(run_test())


if __name__ == '__main__':
    unittest.main()