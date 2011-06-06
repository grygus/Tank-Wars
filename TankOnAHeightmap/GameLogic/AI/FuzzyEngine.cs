using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace TanksOnAHeightmap.GameLogic.AI
{
    public class FuzzyEngine
    {

        public FuzzyEngine()
        {
            FuzzyParameters = new Dictionary<string, float>();
            FuzzyParameters.Add("small", 0.0f);
            FuzzyParameters.Add("normal", 0.0f);
            FuzzyParameters.Add("big", 0.0f);
            FuzzyParameters.Add("aggression", 0.0f);
        }

        float danger      = 50.0f;
        float prayPrior   = 1.0f;
        float healthPrior = 1.0f;

        static public float FuzzyEnemyWeight
        {
            get { return _fuzzyEnemyWeight; }
            set
            {
                _fuzzyEnemyWeight = MathHelper.Clamp(value, 0, 1);
            }
        }
        static float _fuzzyEnemyWeight = .5f;

        static public float FuzzyPreyWeight
        {
            get { return _fuzzyPreyWeight; }
            set
            {
                _fuzzyPreyWeight = MathHelper.Clamp(value, 0, 1);
            }
        }
        static float _fuzzyPreyWeight = .5f;

        static public float FuzzyHealthWeight
        {
            get { return _fuzzyHealthWeight; }
            set
            {
                _fuzzyHealthWeight = MathHelper.Clamp(value, 0, 1);
            }
        }
        static float _fuzzyHealthWeight = .5f;

        //BMK dict
        public Dictionary<string, float> FuzzyParameters { get; set; }

        #region Fuzzy Logic
        float Owa(List<float> rules, List<float> weights)
        {
            List<float> values = new List<float>(rules);
            values.Sort(delegate(float num1, float num2)
            { return num2.CompareTo(num1); });
            //values.Reverse();
            float r = 0.0f;
            for (int i = 0; i < values.Count; i++)
            {
                r += values[i] * weights[i];
            }
            return r;
        }

        void TestOwa()
        {
            List<float> rules = new List<float>();
            List<float> weights = new List<float>();
            rules.Add(1);
            rules.Add(2);

            weights.Add(1);
            weights.Add(4);
            //System.Console.WriteLine(Owa(rules,weights));
        }

        List<float> SoftAndOperator(int n)
        {
            List<float> weights = new List<float>();
            for (int i = 0; i < n; i++)
            {
                weights.Add((2f * (i + 1f)) / (n * (n + 1f)));
            }
            return weights;
        }

        List<float> TestSoftAndOperator()
        {
            return SoftAndOperator(4);
        }

        public float DecideDirection(Matrix unitWorld,Vector3 enemyPosition,Vector3 healthPosition,Vector3 prayPosition)
        {
            float bestDir = .0f, bestEval = .0f;
            float currDir = -(float)Math.PI, currEval = .0f;
            const int nDir = 100;

            //FOR TEST
            Vector3 enemy = new Vector3(200, 0, 200);
            Vector3 own = new Vector3(0, 0, 0);
            Vector3 health = new Vector3(-400, 0, -400);
            Vector3 prey = new Vector3(30, 0, 30);
            //
            //FOR GAME
            Matrix local = Matrix.Invert(unitWorld);

            enemy = Vector3.Transform(enemyPosition, local);
            own = new Vector3(0, 0, 0); //Transformation.Translation;
            health = Vector3.Transform(healthPosition, local);
            prey = Vector3.Transform(prayPosition, local);
            if (float.IsNaN(enemy.X))
                return 0;

            for (int i = 0; i < nDir; i++)
            {
                currEval = EvaluateDirection(currDir, enemy, own, health, prey);
                if (currEval > bestEval && currEval < 1.0)
                {
                    bestDir = currDir;
                    bestEval = currEval;
                }
                //System.Console.WriteLine(currEval);
                currDir += 2 * (float)Math.PI / nDir;
            }
            return bestDir;
        }

        float WeightCriterion(float criterion, float weight)
        {
            float result;
            if ((criterion == 0) && (weight == 0))
            {
                result = 1.0f;
                
            }
            else
            {
                result = (float)Math.Pow(criterion, weight);
            }
            return result;
            //return criterion * weight;
        }

        float FuzzyRadar(float input)
        {
            const int radarDistance = 1200;
            float memDegree;
            if (input < radarDistance)
            {
                memDegree = -input * (1.0f / radarDistance) + 1;
            }
            else
            {
                memDegree = 0.0f;
            }
            return memDegree;
        }
        void TestFuzzyRadar()
        {
            System.Console.WriteLine(FuzzyRadar(0));
            System.Console.WriteLine(FuzzyRadar(150));
            System.Console.WriteLine(FuzzyRadar(250));
            System.Console.WriteLine(FuzzyRadar(400));
        }
        float FuzzySight(float input)
        {
            const float sightDistance = 600;
            float memDegree;
            if (input < sightDistance)
            {
                memDegree = -input * (1.0f / sightDistance) + 1;
            }
            else
            {
                memDegree = 0.0f;
            }
            return memDegree;
        }
        void TestFuzzySight()
        {
            System.Console.WriteLine(FuzzySight(0));
            System.Console.WriteLine(FuzzySight(150));
            System.Console.WriteLine(FuzzySight(250));
            System.Console.WriteLine(FuzzySight(400));
        }
        float FuzzyField(float inX, float inY)
        {
            inX = Math.Abs(inX);
            inY = Math.Abs(inY);

            int width = 2048;
            int height = 2048;

            int width2 = width / 2;
            int height2 = height / 2;

            float memDegree;

            if ((inX <= width2) && (inY <= height2))
            {
                memDegree = 1;
            }
            else if ((inX >= width) || (inY >= height))
            {
                memDegree = 0;
            }
            else
            {
                if (inX >= inY)
                {
                    memDegree = -inX * (2.0f / width) + 2;
                }
                else
                {
                    memDegree = -inY * (2.0f / height) + 2;
                }
            }

            return memDegree;

        }
        void TestFuzzyField()
        {
            System.Console.WriteLine(FuzzyField(3000, 200));
            System.Console.WriteLine(FuzzyField(-1000, 1000));
            System.Console.WriteLine(FuzzyField(1500, 1500));
            System.Console.WriteLine(FuzzyField(200, 0));
        }
        
        float FuzzyDirection(float rad)
        {
            float p1 = -(float)Math.PI;
            float p2 = 0.0f;
            float p3 = (float)Math.PI;
            float memDegree;


            if (rad < p2)
            {
                if (rad < p1)
                {
                    memDegree = rad * (1.0f / p3) + 2;
                }
                else
                {
                    memDegree = -rad * (1.0f / p3);
                }
            }
            else
            {
                if (rad > p3)
                {
                    memDegree = -rad * (1.0f / p3) + 2;
                }
                else
                {
                    memDegree = rad * (1.0f / p3);
                }
            }

            return memDegree;
        }
        void TestFuzzyDirection()
        {
            System.Console.WriteLine(FuzzyDirection((float)Math.PI));
            System.Console.WriteLine(FuzzyDirection(-(float)Math.PI));
            System.Console.WriteLine(FuzzyDirection(-1));
            System.Console.WriteLine(FuzzyDirection(1));
            System.Console.WriteLine(FuzzyDirection(0));
        }
        
        float EvaluateDirection(float direction, Vector3 enemy,
                                                Vector3 own,
                                                Vector3 health,
                                                Vector3 prey)
        {
            Vector3 distance;
            float de, dp, dw;
            float phiE, phiP, phiW;



            distance = enemy - own;
            de = distance.Length();
            phiE = Math.Sign(distance.X) * (float)Math.Acos(distance.Z / de);

            distance = prey - own;
            dp = distance.Length();
            phiP = Math.Sign(distance.X) * (float)Math.Acos(distance.Z / dp);

            distance = health - own;
            dw = distance.Length();
            phiW = Math.Sign(distance.X) * (float)Math.Acos(distance.Z / dw);

            List<float> members = new List<float>();
            //Additional
            FuzzyParameters["prayPriority"] = prayPrior * FuzzySight(dp);
            FuzzyParameters["healthPriority"] = healthPrior * FuzzyRadar(dw);
            FuzzyParameters["enemyPriority"] = FuzzyGrade(danger, 0, 100) * FuzzySight(de);


            float wEnemy;
            if (danger <= 0)
            {
                wEnemy = FuzzyReverseGrade(danger, 0, 50) * 2;
                wEnemy *= WeightCriterion(FuzzyDirection(phiE - direction), wEnemy * FuzzySight(de));
            }
            else
            {
                wEnemy = FuzzyGrade(danger, 0, 100) * FuzzyEnemyWeight;
                wEnemy = WeightCriterion(1.0f - FuzzyDirection(phiE - direction), wEnemy * FuzzySight(de));
            }
            //System.Console.WriteLine(danger);

            //members.Add(wEnemy * WeightCriterion(FuzzyDirection(phiE - direction),
            //                            FuzzySight(de)));
            members.Add(wEnemy);
            //members.Add(WeightCriterion(FuzzyDirection(phiP - direction),
            //                            FuzzySight(dp)));
            members.Add(WeightCriterion(1.0f - FuzzyDirection(phiP - direction),
                                       FuzzyPreyWeight * prayPrior * FuzzySight(dp)));
            members.Add(WeightCriterion(1.0f - FuzzyDirection(phiW - direction),
                                       FuzzyHealthWeight * healthPrior * FuzzyRadar(dw)));
            //members.Add(WeightCriterion(FuzzyField(own.Z+(float)Math.Cos(direction),own.X+(float)Math.Sin(direction)),
            //                            FuzzySight(de)));


            return Owa(members, SoftAndOperator(3));
        }
        #region FuzzySteeringForce
        
        void TestFuzzyForceControll()
        {
            //System.Console.WriteLine(FuzzyForceControll((float)Math.PI));
            System.Console.WriteLine(FuzzyForceControll((float)MathHelper.ToRadians(5)));
        }

        float FuzzyForceControll(float radians)
        {

            //Vector3 distance = Vector3.Transform(tank.ForwardVector,Transformation);
            //Vector3 distance = new Vector3(0,0,-1);
            //dw = distance.Length();
            //phiW = Math.Sign(distance.X) * (float)Math.Acos(distance.Z / dw);
            //float phi = (float)Math.PI;

            radians = MathHelper.ToDegrees(radians);
            if (radians < 0)
                radians = -180 - radians;
            else
                radians = 180 - radians;

            float mFarLeft = FuzzyReverseGrade(radians, -40, -30);
            float mLeft = FuzzyTrapezoid(radians, -40, -25, -15, 0);
            float mAhead = FuzzyTriangle(radians, -10, 10);
            float mRight = FuzzyTrapezoid(radians, 0, 15, 25, 40);
            float mFarRight = FuzzyGrade(radians, 30, 40);
            //System.Console.WriteLine(mFarLeft);
            //System.Console.WriteLine(mLeft);
            //System.Console.WriteLine(mAhead);
            //System.Console.WriteLine(mRight);
            //System.Console.WriteLine(mFarRight);
            float FL = -100;
            float FR = 100;
            return mFarLeft * FL + mLeft * FL + mRight * FR + mFarRight * FR;
        }
        #endregion

        #region FuzzyMembershipFunctions
        float FuzzyGrade(float value, float p1, float p2)
        {
            float result;
            if (p1 > p2)
            {
                float tmp = p2;
                p2 = p1;
                p1 = tmp;
            }

            if (value < p1)
                result = 0;
            else if (value > p2)
                result = 1;
            else
                result = (value - p1) / (p2 - p1);
            return result;
        }
        float FuzzyGradeArg(float value, float p1, float p2)
        {
            float result;
            if (p1 > p2)
            {
                float tmp = p2;
                p2 = p1;
                p1 = tmp;
            }

            if (value < 0)
                result = 0;
            else if (value > 1)
                result = 1;
            else
                result = value * (p2 - p1) + p1;

            return result;
        }
        float FuzzyReverseGrade(float value, float p1, float p2)
        {
            float result;
            if (p1 > p2)
            {
                float tmp = p2;
                p2 = p1;
                p1 = tmp;
            }

            if (value < p1)
                result = 1;
            else if (value > p2)
                result = 0;
            else
                result = (value - p2) / (p1 - p2);
            return result;
        }
        float FuzzyTrapezoid(float value, float p1, float p2, float p11, float p22)
        {
            float result;
            if (value < p2)
                result = FuzzyGrade(value, p1, p2);
            else if (value > p11)
                result = FuzzyReverseGrade(value, p11, p22);
            else
                result = 1;
            return result;
        }
        float FuzzyTriangle(float value, float p1, float p2)
        {
            float result;
            //if (value < p2 && value > p1)
            //{
            float p3 = (p2 - p1) / 2f + p1;
            if (value < p3)
                result = FuzzyGrade(value, p1, p3);
            else
                result = FuzzyReverseGrade(value, p3, p2);
            //}
            //else
            //result = 0;
            return result;
        }
        #endregion
        #region FuzzyDecision
        
        float FuzzyAttackDecision(float value, float value2)
        {
            List<float> myHealth = new List<float>();
            myHealth.Add(FuzzyReverseGrade(value, 0, 50)); //ND
            myHealth.Add(FuzzyTriangle(value, 25, 75));//Good
            myHealth.Add(FuzzyGrade(value, 50, 100));//Excellent

            List<float> enemyHealth = new List<float>();
            enemyHealth.Add(FuzzyReverseGrade(value2, 0, 50)); //ND
            enemyHealth.Add(FuzzyTriangle(value2, 25, 75));//Good
            enemyHealth.Add(FuzzyGrade(value2, 50, 100));//Excellent

            List<float> attack = new List<float>();
            List<float> fullAttack = new List<float>();
            List<float> run = new List<float>();
            float min;
            float maxAttack = 0.0f;
            float maxRun = 0.0f;
            float maxFullAttack = 0.0f;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    min = Math.Min(myHealth[i], enemyHealth[j]);

                    if (i == 0)
                        if (j == 0)
                            attack.Add(min);
                        else
                            run.Add(min);
                    else if (i == 1)
                        if (j == 2)
                            run.Add(min);
                        else if (j == 0)
                            fullAttack.Add(min);
                        else
                        {
                            attack.Add(min);

                        }
                    else
                        if (j == 2)
                            attack.Add(min);
                        else
                            fullAttack.Add(min);
                }
            }

            foreach (float val in attack)
                maxAttack = Math.Max(maxAttack, val);

            foreach (float val in run)
                maxRun = Math.Max(maxRun, val);


            foreach (float val in fullAttack)
                maxFullAttack = Math.Max(maxFullAttack, val);

            FuzzyParameters["small"] = maxFullAttack;
            FuzzyParameters["normal"] = maxAttack;
            FuzzyParameters["big"] = maxRun;
            //if (maxRun > maxAttack)
            //    maxRun *= 2;
            //else
            //    maxAttack *= 2;
            //else if(maxFullAttack > maxRun && maxFullAttack > maxAttack)
            //    maxFullAttack *= 2;

            //System.Console.WriteLine("maxFullAttack");
            //System.Console.WriteLine(maxFullAttack);
            //System.Console.WriteLine("maxAttack");
            //System.Console.WriteLine(maxAttack);
            //System.Console.WriteLine("maxRun");
            //System.Console.WriteLine(maxRun);
            FuzzyParameters["aggression"] = FuzzyAttackIntegrate(maxRun, maxAttack, maxFullAttack);
            return FuzzyParameters["aggression"];
        }

        float FuzzyAttackIntegrate(float maxRun, float maxAttack, float maxFullAttack)
        {
            float dt = 0.01f;
            float result = 0.0f;
            float denominator = 0.0f;
            float tmpValue;
            for (float i = 0; i <= 100; i += dt)
            {
                if (i < 30)
                {
                    tmpValue = Math.Min(FuzzyTriangle(i, 0, 50), maxRun);
                    result += i * tmpValue;
                    denominator += tmpValue;
                }
                else if (i < 40)
                {
                    tmpValue = Math.Max(Math.Min(FuzzyTriangle(i, 0, 50), maxRun),
                                        Math.Min(FuzzyTriangle(i, 30, 70), maxAttack));
                    result += i * tmpValue;
                    denominator += tmpValue;
                }
                else if (i < 60)
                {
                    tmpValue = Math.Min(FuzzyTriangle(i, 30, 70), maxAttack);
                    result += i * tmpValue;
                    denominator += tmpValue;
                }
                else if (i < 70)
                {
                    tmpValue = Math.Max(Math.Min(FuzzyTriangle(i, 60, 100), maxFullAttack),
                                        Math.Min(FuzzyTriangle(i, 30, 70), maxAttack));
                    result += i * tmpValue;
                    denominator += tmpValue;
                }
                else
                {
                    tmpValue = Math.Min(FuzzyTriangle(i, 60, 100), maxFullAttack);
                    result += i * tmpValue;
                    denominator += tmpValue;
                }
            }
            return result / denominator;
        }

        float FuzzyPrayDecision(float damage)
        {

            return FuzzyGrade(damage, 0, 100);
        }

        float FuzzyHealthDecision(float health)
        {
            return FuzzyReverseGrade(health, 0, 100) * 4;
        }

        public void UpdateParameters(int unitLife,int enemyLife)
        {
            danger = FuzzyAttackDecision(unitLife, enemyLife);
            prayPrior = FuzzyPrayDecision(100);
            healthPrior = FuzzyHealthDecision(unitLife);
        }
        #endregion

        #region ErrorLogging
        
        void add()
        {
            Trace.WriteLine("hello");
        }
        #endregion
        #endregion

    }
}
