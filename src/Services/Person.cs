using System;
using System.Linq;
using covidSim.Models;

namespace covidSim.Services
{
    public class Person
    {
        private const int MaxDistancePerTurn = 30;
        private static Random random = new Random();
        private readonly CityMap map;
        private PersonState state = PersonState.AtHome;
        private readonly CityMap map;
        public bool IsSick;


        public Person(int id, int homeId, CityMap map, bool isSick)
        {
            Id = id;
            HomeId = homeId;
            this.map = map;
            IsSick = isSick;
            if (isSick)
                StepsToRecovery = 35;

            this.map = map;

            var homeCoords = map.Houses[homeId].Coordinates.LeftTopCorner;
            var x = homeCoords.X + random.Next(HouseCoordinates.Width);
            var y = homeCoords.Y + random.Next(HouseCoordinates.Height);
            Position = new Vec(x, y);
        }
        
        public int Id;
        public int HomeId;
        public Vec Position;
        public int StepsToRecovery;

        public void CalcNextStep()
        {
            StepsToRecovery--;
            if (StepsToRecovery == 0)
                IsSick = false;
            
            switch (state)
            {
                case PersonState.AtHome:
                    CalcNextStepForPersonAtHome();
                    break;
                case PersonState.Walking:
                    CalcNextPositionForWalkingPerson();
                    break;
                case PersonState.GoingHome:
                    CalcNextPositionForGoingHomePerson();
                    break;
            }
        }

        private void CalcNextStepForPersonAtHome()
        {
            var goingWalk = random.NextDouble() < 0.005;
            if (!goingWalk)
                CalcNextPositionForStayingHomePerson();
            else
            {
                state = PersonState.Walking;
                CalcNextPositionForWalkingPerson();
            }

        }

        private void CalcNextPositionForStayingHomePerson()
        {
            var nextPosition = GenerateNextRandomPosition();

            if (isCoordInField(nextPosition) && IsCoordsInHouse(nextPosition))
                Position = nextPosition;
        }

        private bool IsCoordsInHouse(Vec vec)
        {
            var houseCoordinates = map.Houses[HomeId].Coordinates.LeftTopCorner;

            return
                vec.X >= houseCoordinates.X && vec.X <= HouseCoordinates.Width+ houseCoordinates.X &&
                vec.Y >= houseCoordinates.Y && vec.Y <= HouseCoordinates.Height+houseCoordinates.Y;
        }

        private Vec GenerateNextRandomPosition()
        {
            var xLength = random.Next(MaxDistancePerTurn);
            var yLength = MaxDistancePerTurn - xLength;
            var direction = ChooseDirection();
            var delta = new Vec(xLength * direction.X, yLength * direction.Y);
            var nextPosition = new Vec(Position.X + delta.X, Position.Y + delta.Y);

            return nextPosition;
        }

        private void CalcNextPositionForWalkingPerson()
        {
            var xLength = random.Next(MaxDistancePerTurn);
            var yLength = MaxDistancePerTurn - xLength;
            var direction = ChooseDirection();
            var delta = new Vec(xLength * direction.X, yLength * direction.Y);
            var nextPosition = new Vec(Position.X + delta.X, Position.Y + delta.Y);

            if (isCoordInField(nextPosition) && !IsCoordInAnyHouse(nextPosition))
            {
                Position = nextPosition;
            }
            else
            {
                CalcNextPositionForWalkingPerson();
            }
        }

        private void CalcNextPositionForGoingHomePerson()
        {
            var game = Game.Instance;
            var homeCoord = game.Map.Houses[HomeId].Coordinates.LeftTopCorner;
            var homeCenter = new Vec(homeCoord.X + HouseCoordinates.Width / 2, homeCoord.Y + HouseCoordinates.Height / 2);

            var xDiff = homeCenter.X - Position.X;
            var yDiff = homeCenter.Y - Position.Y;
            var xDistance = Math.Abs(xDiff);
            var yDistance = Math.Abs(yDiff);

            var distance = xDistance + yDistance;
            if (distance <= MaxDistancePerTurn)
            {
                Position = homeCenter;
                state = PersonState.AtHome;
                return;
            }

            var direction = new Vec(Math.Sign(xDiff), Math.Sign(yDiff));

            var xLength = Math.Min(xDistance, MaxDistancePerTurn); 
            var newX = Position.X + xLength * direction.X;
            var yLength = MaxDistancePerTurn - xLength;
            var newY = Position.Y + yLength * direction.Y;
            Position = new Vec(newX, newY);
        }

        public void GoHome()
        {
            if (state != PersonState.Walking) return;

            state = PersonState.GoingHome;
            CalcNextPositionForGoingHomePerson();
        }

        private Vec ChooseDirection()
        {
            var directions = new Vec[]
            {
                new Vec(-1, -1),
                new Vec(-1, 1),
                new Vec(1, -1),
                new Vec(1, 1),
            };
            var index = random.Next(directions.Length);
            return directions[index];
        }

        private bool isCoordInField(Vec vec)
        {
            var belowZero = vec.X < 0 || vec.Y < 0;
            var beyondField = vec.X > Game.FieldWidth || vec.Y > Game.FieldHeight;

            return !(belowZero || beyondField);
        }

        private bool IsCoordInAnyHouse(Vec vec) => map.Houses.Any(h => IsCoordInHouse(vec, h));

        private static bool IsCoordInHouse(Vec vec, House house)
        {
            return vec.X > house.Coordinates.LeftTopCorner.X &&
                   vec.X < house.Coordinates.LeftTopCorner.X + HouseCoordinates.Width &&
                   vec.Y > house.Coordinates.LeftTopCorner.Y &&
                   vec.Y < house.Coordinates.LeftTopCorner.Y + HouseCoordinates.Height;
        }
    }
}