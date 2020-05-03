using System;
using System.Linq;
using covidSim.Models;

namespace covidSim.Services
{
    public class Person
    {
        private const int MaxDistancePerTurn = 30;
        private const int InitialStepsToRecovery = 35;
        private const int InitialStepsToRot = 10;
        private const double ProbabilityOfDying = 0.000003;
        private static Random random = new Random();
        public PersonHealth Health = PersonHealth.Healthy;
        private readonly CityMap map;

        internal PersonState State { get; private set; } = PersonState.AtHome;
        
        public Person(int id, int homeId, CityMap map, bool isSick)
        {
            Id = id;
            HomeId = homeId;
            IsSick = isSick;
            IsBored = false;
            timeAtHome = 0;
            this.map = map;
            if (isSick) 
                ChangeHealth(PersonHealth.Sick);

            var homeCoords = map.Houses[homeId].Coordinates.LeftTopCorner;
            var x = homeCoords.X + random.Next(HouseCoordinates.Width);
            var y = homeCoords.Y + random.Next(HouseCoordinates.Height);
            Position = new Vec(x, y);
        }

        public int Id;
        public int HomeId;
        public Vec Position;
        public bool IsSick;

        public bool IsBored;
        
        private int timeAtHome;
      
        public int StepsToRecovery;
        public int StepsToRot;

        public bool OutOfTheGame => Health == PersonHealth.Dead && StepsToRot == 0;

        public void CalcNextStep()
        {
            if (CalcIsAtHome())
                timeAtHome += 1;
            else
            {
                timeAtHome = 0;
            }
            IsBored = timeAtHome >= 5;
       
            switch (Health)
            {
                case PersonHealth.Dead:
                    StepsToRot--;
                    return;
                case PersonHealth.Sick:
                {
                    StepsToRecovery--;
                    if (StepsToRecovery == 0)
                        Health = PersonHealth.Healthy;
                    else if (TryToDie())
                        return;
                    break;
                }
            }
            Move();
        }

        private void Move()
        {
            switch (State)
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

        private bool TryToDie()
        {
            if (random.NextDouble() > ProbabilityOfDying) return false;
            ChangeHealth(PersonHealth.Dead);
            return true;
        }

        public void ChangeHealth(PersonHealth next)
        {
            Health = next;
            switch (next)
            {
                case PersonHealth.Sick:
                    StepsToRecovery = InitialStepsToRecovery;
                    break;
                case PersonHealth.Dead:
                    StepsToRot = InitialStepsToRot;
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
                State = PersonState.Walking;
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

        private bool CalcIsAtHome()
        {
            var game = Game.Instance;
            var homeCoordLeft = game.Map.Houses[HomeId].Coordinates.LeftTopCorner;
            var homeWidth = HouseCoordinates.Width;
            var homeHeight = HouseCoordinates.Height;
            if (Position.X < homeCoordLeft.X || Position.X >= homeCoordLeft.X + homeWidth)
                return false;
            if (Position.Y < homeCoordLeft.Y || Position.Y >= homeCoordLeft.Y + homeHeight)
                return false;
            return true;
        }

        private void CalcNextPositionForGoingHomePerson()
        {
            var game = Game.Instance;
            var homeCoord = game.Map.Houses[HomeId].Coordinates.LeftTopCorner;
            var homeCenter = new Vec(homeCoord.X + HouseCoordinates.Width / 2,
                homeCoord.Y + HouseCoordinates.Height / 2);

            var xDiff = homeCenter.X - Position.X;
            var yDiff = homeCenter.Y - Position.Y;
            var xDistance = Math.Abs(xDiff);
            var yDistance = Math.Abs(yDiff);

            var distance = xDistance + yDistance;
            if (distance <= MaxDistancePerTurn)
            {
                Position = homeCenter;
                State = PersonState.AtHome;
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
            if (State != PersonState.Walking) return;

            State = PersonState.GoingHome;
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
