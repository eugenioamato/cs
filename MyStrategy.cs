using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;


namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {
        public Action[] acts;
        public int turn = 0;
        public int robotsDone = 0;
        public static Rules rules;
        public static Arena ar;
        public Sgame game;
        public static int firstrobot = 1;
        public static int firstenemyrobot = 3;
        public int pause;
        public double sfals = 0;
        public bool nogood;
        public double start = 0;
        public static string printout = "";
        public System.Random rng = new System.Random();
        public static Dan dw;
        public static Vec bal,afterball, afterball1;
        public static Vec[] robs;
        public static double aw2, ag2, ad2, aw2cr, ad2cr;
        public static Vec goalcorner,vec0,vec1,vec2,vec3,vec4,vec5,vec6,corner_o;
        public static int robotsPerPlayer=2;
        public static double oneontickspersecond;
        public static double averagehit;

        public static void InitConstants()
        {
            robotsPerPlayer = rules.team_size;
            oneontickspersecond = 1.0 / rules.TICKS_PER_SECOND;
            
            averagehit = rules.MAX_HIT_E + rules.MIN_HIT_E / 2;
            aw2 = ar.width / 2;
            aw2cr = aw2 - ar.corner_radius;
            ag2 = ar.goal_width / 2;
            ad2 = ar.depth / 2;
            ad2cr = ad2 - ar.corner_radius;

            goalcorner = new Vec(ag2 - ar.goal_top_radius, ar.goal_height - ar.goal_top_radius, 0);
            vec0 = new Vec(0, 0, 0);
            vec1 = new Vec(0, 1, 0);
            vec2 = new Vec(0, ar.height, 0);
            vec3 = new Vec(0, -1, 0);
            vec4 = new Vec(0, 0, ad2);
            vec5 = new Vec(0, 0, -1);
            vec6 = new Vec(aw2, 0, 0);
            corner_o= new Vec(aw2cr,
                                 ad2cr, 0);
        }

        public static bool HasValue(double value)
        {
            return !System.Double.IsNaN(value) && !System.Double.IsInfinity(value);
        }

        public class Sgame
        {
            public int current_tick;
            public Player[] players;
            public Vec[,] robot;
            
            public NitroPack[] nitro_packs;
            public Vec ball;

            public Sgame(Game g)
            {
                current_tick = g.current_tick;
                ball = new Vec(g.ball);

                players = new Player[2];
                for (int i = 0; i < 2; i++)
                    players[i] = CopyPlayer(g.players[i]);

                robot = new Vec[2,robotsPerPlayer];

                for (int i = 0; i < g.robots.Length; i++)
                {
                    bool mine = g.robots[i].is_teammate;
                    int index = g.robots[i].id - (mine?firstrobot:firstenemyrobot);
                    robot[mine ? 0 : 1,index] = new Vec(g.robots[i]);
                }

                nitro_packs = new NitroPack[g.nitro_packs.Length];
                for (int i = 0; i < g.nitro_packs.Length; i++)
                    nitro_packs[i] = CopyNitro(g.nitro_packs[i]);


            }

            public Sgame(Sgame g, bool fast)
            {
                ball = new Vec(g.ball);



                robot = new Vec[robotsPerPlayer, 2];

                for (int i = 0; i < robotsPerPlayer; i++)
                    for (int j = 0; j < 2; j++)
                    {
                        robot[i, j] = new Vec(g.robot[i, j]);
                    }
                current_tick = g.current_tick;


                if (!fast)
                {
                    nitro_packs = new NitroPack[g.nitro_packs.Length];
                    for (int i = 0; i < g.nitro_packs.Length; i++)
                        nitro_packs[i] = CopyNitro(g.nitro_packs[i]);
                    players = new Player[2];
                    for (int i = 0; i < 2; i++)
                        players[i] = CopyPlayer(g.players[i]);
                }
                else
                {
                    nitro_packs = g.nitro_packs;
                    players = g.players;

                }
            


            }


        }


        public class Dan : System.IComparable
        {
            public double distance;
            public Vec normal;

            public Dan(double dist, Vec norm)
            {
                distance = dist;
                normal = new Vec(norm);
            }

            public int CompareTo(object o)
            {
                Dan d = (Dan)o;
                double dd = (distance - d.distance);
                return dd < 0 ? -1 : dd > 0 ? 1 : 0;   
            }

            public Vec ToVec()
            {
                return new Vec(normal.Normalize(distance));
            }

            public static Dan Min(Dan a, Dan b)
            {
                bool r = a.distance < b.distance;
               // if (!r) MyStrategy.De("better "+b.distance);
                return  r? a : b;
            }

            public static bool operator >(Dan left, Dan right) => left.distance > right.distance;
            public static bool operator <(Dan left, Dan right) => left.distance < right.distance;

            public override string ToString()
            {
                return normal.x.ToString("0,0") + " " 
                    + normal.y.ToString("0,0") + " " 
                    + normal.z.ToString("0,0") + 
                    " L: " + distance;
            }
        }

        public class Vec
        {
            public double x;
            public double y;
            public double z;
            public double vx;
            public double vy;
            public double vz;
            public double r;
            public double m=1;
            public double rcs;

            public Vec(double nx, double ny, double nz, double nvx, double nvy, double nvz, double nr, double nm)
            {
                x = nx; y = ny; z = nz;
                vx = nvx; vy = nvy; vz = nvz;
                r = nr; m = nm;
            }

            public Vec(Vec b) : this(b.x, b.y, b.z, b.vx, b.vy, b.vz, b.r, b.m)
            {
            }

            public Vec(double nx, double ny, double nz, double nvx, double nvy, double nvz) :
                this(nx, ny, nz, nvx, nvy, nvz, 0, 0)
            { }


            public Vec(double ax, double ay, double az) : this(ax, ay, az, 0, 0, 0, 0, 0)
            {
            }

            public Vec Invert()
            {
                return new Vec(-x, -y, -z);
            }

            public Vec(Robot ro)
            {
                x = ro.x;
                y = ro.y;
                z = ro.z;
                vx = ro.velocity_x;
                vy = ro.velocity_y;
                vz = ro.velocity_z;
                r = ro.radius;
                m = rules.ROBOT_MASS;
                
            }

            public Vec(Ball b)
            {
                x = b.x;
                y = b.y;
                z = b.z;
                vx = b.velocity_x;
                vy = b.velocity_y;
                vz = b.velocity_z;
                r = b.radius;
                m = rules.BALL_MASS;
            }

            public Ball ToBall()
            {
                Ball b = new Ball();
                b.x = x; b.y = y; b.z = z; 
                b.velocity_x = vx; b.velocity_y = vy; b.velocity_z = vz;
                b.radius = r; 
                return b;
            }

            public Vec(Action a)
            {
                vx = a.target_velocity_x;
                vy = a.target_velocity_y;
                vz = a.target_velocity_z;
            }

            public void AddVelocity(Vec a)
            {

                vx += a.x;
                vy += a.y;
                vz += a.z;

            }

            public void RemVelocity(Vec a)
            {

                vx -= a.x;
                vy -= a.y;
                vz -= a.z;

            }



            public Vec VelocityVec()
            {
                return new Vec(vx, vy, vz);
            }

            public double Velocity()
            {
                return (System.Math.Sqrt(vx * vx + vy * vy + vz * vz));
            }

            public Vec Moved(double time)
            {
                if (Velocity() > MyStrategy.rules.MAX_ENTITY_SPEED)
                {
                    DoNormalizeVel(MyStrategy.rules.MAX_ENTITY_SPEED);
                }

                double nx = x + (vx * time);
                double ny = y + (vy * time);
                double nz = z + (vz * time);
                return new Vec(nx, ny, nz, vx, vy, vz, r, m);
            }

            public void Move(double time)
            {
                
                if (Velocity() > MyStrategy.rules.MAX_ENTITY_SPEED)
                {
                    DoNormalizeVel(MyStrategy.rules.MAX_ENTITY_SPEED);
                
                }

                x += vx*time;
                z += vz*time;

                /*if ((             //TODO IMPROVE GROUND MOVEMENT
                    (vy<0)&&(vy>-1)&&(y-r>-0.01)&& (y-r<0)
                    ))
                {
                    vy = 0;
                    y = r;
                }
                else*/
                {
                    y += vy * time;
                    this.y -= rules.GRAVITY * time * time / 2;
                    this.vy -= rules.GRAVITY * time;
                }
            }




            public double Dot(Vec b)
            {
                return (x * b.x + y * b.y + z * b.z);
            }

            public Dan DanToPlane(Vec point_on_plane, Vec plane_normal)
            {
                double distance = this.Sub(point_on_plane).Dot(plane_normal);
                Vec rnormal = new Vec(plane_normal);

                return new Dan(distance, rnormal);
            }

            public Dan DanToSphereInner(Vec sphere_center, double sphere_radius)
            {
                double distance = sphere_radius - this.Sub(sphere_center).Len();
                Vec normal = sphere_center.Sub(this).Normalize();
                if (distance < 0)
                {
                    /*
                    De("fail center= " + sphere_center.x + " , "
                        + sphere_center.y + " , " + sphere_center.z + " ln " + distance);
                    De("my " + x + " , " + y + " , " + z);
                    De("normal " + normal.ToString());
                    */
                    if (sphere_center.x > 80) return new Dan(9999, normal);
                    else
                        return new Dan(-distance, normal);
                }
                /*
                    return new Dan(-distance, normal.Invert());
                else*/
                    return new Dan(distance, normal);
            }

            public Dan DanToSphereOuter(Vec sphere_center, double sphere_radius)
            {
                double distance = this.Sub(sphere_center).Len() - sphere_radius;
                Vec normal = this.Sub(sphere_center).Normalize();
                /*if (distance < 0)
                    return new Dan(-distance, normal.Invert());
                else*/
                    return new Dan(distance, normal);
            }

            

            public Dan DanToArenaQuarter()
            {
                Vec simplifiedVec=new Vec(x, y, 0);
                //ground
                Dan dan = new Dan(y, vec1);
                if (y > 10)
                    dan = new Dan(ar.height - y, vec3);

                //De("ground " + dan);
                //Ceiling
                //De("Tetto");
                /*
                Dan ddp = DanToPlane(vec2,
                               vec3);

                dan = Dan.Min(dan,
                    ddp
                    );
                 De("tetto "+ddp.distance) ;*/

                //side X
                
                //De("side X");
                dan = Dan.Min(dan,
                    DanToPlane(vec6,
                               new Vec(-1,0,0))
                    );
                
                /*side z (goal)
                dan = Dan.Min(dan,
                    DanToPlane(new Vec(0,0,ar.depth/2+ar.goal_depth),
                               new Vec(0,0,-1))
                );*/
                //size z

                //De("side z");
                Vec v = this.Sub(goalcorner);
                if (
                    (v.x>=(ag2)-ar.goal_side_radius) || 
                    (v.y>ar.goal_height+ar.goal_side_radius) ||
                    ((v.x>0) && (v.y>0) && (v.Len()>=ar.goal_top_radius+ar.goal_side_radius))
                    )

                dan = Dan.Min(dan,
                    DanToPlane(vec4,vec5)
                );
                //corner
                
                //De("Corner");
                if (
                    (x > aw2cr) && (z > ad2cr)
                    )
                    {

                    Dan cornerDan = DanToSphereInner(new Vec(aw2cr, y,ad2cr), ar.corner_radius);
                        dan = Dan.Min(dan,
                                cornerDan
                            );
                    }
                    // goal outer corner
                if (z<ad2+ar.goal_side_radius)
                {
                    //side x

                    //De("palo");
                    if (x < ag2 + ar.goal_side_radius)
                        dan = Dan.Min(dan,
                            DanToSphereOuter(new Vec(ag2+ar.goal_side_radius,y,
                            ad2+ar.goal_side_radius
                            ),ar.goal_side_radius)
                            );

                    // ceiling

                    //De("Traversa tetto");
                    if (y < ar.goal_height + ar.goal_side_radius)
                        dan = Dan.Min(dan,
                            DanToSphereOuter(new Vec(x,ar.goal_height+ar.goal_side_radius,
                            ad2+ar.goal_side_radius),ar.goal_side_radius)
                            );
                    //top corner side

                    //De("Traversa angolo alto");
                    
                    Vec vv = this.Sub(goalcorner);
                    if ((vv.x>0) && (vv.y>0))
                    {
                        goalcorner = goalcorner.Add(vv.Normalize().Mul(ar.goal_top_radius + ar.goal_side_radius));
                        dan = Dan.Min(dan,
                            DanToSphereOuter(new Vec(x, y, ad2 + ar.goal_side_radius), ar.goal_side_radius)
                            );
                    }
                }

                
                //bottom curls
                if (y<ar.bottom_radius)
                {
                    //x
                    //De("bottom corners: side x");
                    if (x>aw2 -ar.bottom_radius)
                        dan = Dan.Min(dan,
                            DanToSphereInner(new Vec(aw2-ar.bottom_radius,ar.bottom_radius,z),
                            ar.bottom_radius)
                            );
                
                    //z
                    //De("side z");
                    if (z > ad2 - ar.bottom_radius)
                        if (x >= ag2 + ar.goal_side_radius)
                            dan = Dan.Min(dan,
                                DanToSphereInner(new Vec(x,ar.bottom_radius,ad2-ar.bottom_radius),
                                ar.bottom_radius)
                                );

                    
                    //goal outer corner
                    Vec O = new Vec(ag2 + ar.goal_side_radius,
                        ad2 + ar.goal_side_radius, 0);
                    Vec V = new Vec(x,y,0).Sub(O);
                    if ((V.x > 0) && (V.y > 0))
                        if (V.Len()<ar.goal_side_radius+ar.bottom_radius)
                    {
                      //  De("goal out corn");
                            O = O.Add(V.Normalize()).Mul(ar.goal_side_radius+ar.bottom_radius);
                            dan = Dan.Min(dan,
                                DanToSphereInner(new Vec(O.x,ar.bottom_radius,O.y),ar.bottom_radius)
                                );
                    }

                    
                    if (x>aw2-ar.corner_radius)
                        if(z>ad2-ar.corner_radius)
                        {
                            
                            Vec n = simplifiedVec.Sub(corner_o);
                            double dist = n.Len();
                            if (dist>ar.corner_radius-ar.bottom_radius)
                            {
                                n = n.Mul(1 / dist);
                                Vec o2 = corner_o.Add(n).Mul(ar.corner_radius -ar.bottom_radius);
                                //De("bottom corner");
                                dan = Dan.Min(dan, DanToSphereInner(new Vec(o2.x,
                                    ar.bottom_radius, o2.y), ar.bottom_radius)
                                    );
                            }
                        }


                    /*
                    //corner bottom
                    if (x > aw2 - ar.corner_radius)
                        if (z > ad2 - ar.corner_radius)
                        {
                            //De("corner bottom");

                            Vec N = simplifiedVec.Sub(corner_o);
                            double dist = N.Len();
                            if (dist > ar.corner_radius - ar.bottom_radius)
                            {
                                N = N.Mul(1 / dist);
                                Vec o2 = corner_o.Add(N).Mul(ar.corner_radius - ar.bottom_radius);
                                dan = Dan.Min(dan,
                                    DanToSphereInner(new Vec(o2.x, ar.bottom_radius, o2.y),
                                    ar.bottom_radius
                                    )
                                    );
                            }
                        }*/
                }

                
                

                //ceiling corners
                if (y>ar.height-ar.top_radius)
                {
                    //side x
                    if (x>aw2 -ar.top_radius)
                    {
                        //De("ceil corn X side");
                        dan = Dan.Min(dan,
                            DanToSphereInner(new Vec(aw2 - ar.top_radius, ar.height - ar.top_radius, z), ar.top_radius)
                            );
                    }

                    //side z
                    if (z>ad2-ar.top_radius)
                    {
                        //De("ceil corn Z side");
                        dan = Dan.Min(dan,
                            DanToSphereInner(new Vec(x,ar.height-ar.top_radius,
                            ad2-ar.top_radius)
                            ,ar.top_radius));

                    }
                    //corner
                    if (x>aw2-ar.corner_radius)
                        if (z>ad2-ar.corner_radius)
                        {
                            Vec corner_o = new Vec(aw2-ar.corner_radius,
                                ad2-ar.corner_radius,0);
                            Vec dv = simplifiedVec.Sub(corner_o);
                            if (dv.Len()>ar.corner_radius-ar.top_radius)
                            {
                                //De("ceil corner");
                                Vec n = dv.Normalize();
                                Vec o2 = corner_o.Add(n).Mul(ar.corner_radius - ar.top_radius);
                                dan = Dan.Min(dan,
                                    DanToSphereInner(new Vec(o2.x,ar.height-ar.top_radius,
                                    o2.y),ar.top_radius));
                            }
                        }

                }


                return dan;


            }

            public Dan DanToArena()
            {
                double ax = x;
                double az = z;

                bool negX = x < 0;
                bool negZ = z < 0;
                if (negX) ax = -x;
                if (negZ) az = -z;

                Dan result;
                if ((!negX) && (!negZ))
                    return DanToArenaQuarter();
                
                result= new Vec(ax, y, az).DanToArenaQuarter();

                if (negX) result.normal.x = -result.normal.x;
                if (negZ) result.normal.z = -result.normal.z;
                return result;

            }

            public double Len()
            {
                return System.Math.Sqrt(Len2());
            }

            public double Len2()
            {
                return x * x + y * y + z * z;
            }

            public Vec Normalize(double to)
            {
                return this.Mul(to / Len());
            }

            public Vec Normalize()
            {
                return Normalize(1);
            }

            public void DoNormalize()
            {
                Vec b = Normalize();
                x = b.x;
                y = b.y;
                z = b.z;
            }

            public void DoNormalize(double g)
            {
                Vec b = Normalize(g);
                x = b.x;
                y = b.y;
                z = b.z;
            }

            public void DoNormalizeVel(double g)
            {
                Vec b = VelocityVec().Normalize(g);
                vx = b.x;
                vy = b.y;
                vz = b.z;

            }

            public Vec Sub(Vec b)
            {
                return new Vec(x - b.x, y - b.y, z - b.z);
            }

            public Vec Add(Vec b)
            {
                return new Vec(x + b.x, y + b.y, z + b.z);
            }

            public void DoAdd(Vec b)
            {
                x += b.x;
                y += b.y;
                z += b.z;
            }

            public Vec Mul(double k)
            {
                return new Vec(x * k, y * k, z * k);
            }

            public override string ToString()
            {
                return x + " , " + y + " " + z +" vel "+vx+" ," +vy+" , "+vz;
            }

        }

        public static void De(object o)
        {
            //printout += o.ToString() + " \\n ";
            //System.Console.Error.WriteLine(o);
        }

        public static void Re(object o)
        {
            //printout += o.ToString() + " \\n ";
            //System.Console.Error.WriteLine(o);
        }



        public Action ReachAction(Vec me, double x, double z, bool iskick)
        {
            double dx = me.x - x;
            double dz = me.z - z;
            Vec v = new Vec(dx, 0, dz);
            if (iskick)
                v = v.Normalize(rules.ROBOT_MAX_GROUND_SPEED);
            else
            {
                v = v.Normalize(rules.ROBOT_MAX_GROUND_SPEED/10 * v.Len());//.Sub(new Vec(me.velocity_x, 0, me.velocity_z));

            }

            Action r = new Action();
            r.target_velocity_x = -v.x;
            r.target_velocity_z = -v.z;



            return r;
        }
        
        /*
        public Robot GetRobot(int i)
        {
            foreach (Robot r in game.robots)
                if (r.id == i + firstrobot)
                    return r;
            
            return null;
        }*/

        public double Dist(double x, double z, double bx, double bz)
        {
            return System.Math.Sqrt(Dist2(x, z, bx, bz));
        }
        public double Dist2(double x, double z, double bx, double bz)
        {
            double dx = x - bx;
            double dz = z - bz;
            return dx * dx + dz * dz;
        }


        public double ggx = 0;

        public Action[] ChooseMoves()
        {

            
            Action[] az = new Action[robotsPerPlayer];
            az[0] = new Action(); az[1] = new Action();

            Vec ball = new Vec(game.ball);


            double gx = game.ball.x;
            double gz = game.ball.z;

            double varied = 0;
            double tilted = 0;

            
            Vec rob1 = game.robot[0, firstrobot - 1];
            //if (m1 == null) return az;

            double d = Dist(rob1.x, rob1.z, gx, gz);
            double dd = rob1.Sub(ball).Len();
            if (pause > 1)
                pause--;
            nogood = false;
            // trova punto di incontro
            // calcola angolo con porta nemica
            double dx = gx - rob1.x;
            double dz = Abs(gz - rob1.z);
            double togo = Abs(40 - gz);
            if (dz == 0) dz = 0.000001;
            sfals =gx/130+( dx / dz);
            //De("sfals " + sfals);
            //printout = sfals.ToString("0.00");
            if (pause > 1)
                if (Abs(rob1.vx - game.ball.vx) < 1)
                    if ((game.ball.y - 1) < dd - rules.BALL_RADIUS)
                        if (Abs(sfals )< 0.2)
                        {
                            pause = 1; //De("pause reset"); 
                        }
            bool center = false;
            if ((Abs(gx) < 0.1))
                if ((Abs(gz) < 0.1))
                    if ((Abs(game.ball.vz) < 0.1))
                    {
                        center = true; //De("center " + sfals); 
                    }
            //De(sfals+" "+dx + " " + dz + " " + togo);
            if (center)
            {
                if (Abs(sfals )> 0.3)
                    nogood = true;
            }
            /*else
                if ((Abs(sfals )> 4.6))
                nogood = true;
            */
            /*if ((game.ball.z - rules.BALL_RADIUS + game.ball.velocity_y / 10 > 2) && (dd < rules.BALL_RADIUS * 4))
                nogood = true;*/

            if ((rob1.z > gz + (game.ball.vz * (dd / pause * 2)) - rules.BALL_RADIUS) || (nogood))
            {
                if ((rob1.z > 0) || (nogood))
                    varied = -(2 + dd / 3 + rules.BALL_RADIUS - (game.ball.vz < 0 ? game.ball.vz / 10 : 0));

                if (rob1.z + varied < -40) varied = 40 + rob1.z;

                nogood = true;


                if ((Abs(rob1.x - gx) < rules.BALL_RADIUS * 2) &&
                        (Abs(rob1.z - gz) < rules.BALL_RADIUS * 2))
                {
                    if (game.ball.y - rules.BALL_RADIUS < 2)
                        tilted = ((game.ball.vx < 0) ? -rules.BALL_RADIUS * 4 : rules.BALL_RADIUS * 4);

                    pause = (int)(dd * 10+game.ball.y);
                    //De("dd=" + dd);
                }
                else
                {
                    //if (m1.z<0)
                    tilted += gx / 2;
                    tilted += game.ball.vx / 10;
                }

            }



            az[0] = ReachAction(rob1, gx + tilted, gz + varied, true);


            //capire se dopo me e la palla c'e' porta nemica
            /*
             * 1 capire la distanza del primissimo possibile tocco
             2 verificare traiettoria
             3 rettificare traiettoria a partire dalle modifiche alla posizione
              -- o spostarsi per ottenere posizione favorevole

             4 elencare tutte le posizioni alternative con tempo di arrivo
             5 verificare impossibilita del nemico di entrare nelle aree coinvolte
             6 se nessuna mossa è uno "scacco matto"
            scegliere quella piu lontana dalle traiettorie ipotetiche dei nemici
            7 se nessuna mossa manda la palla in porta
            avvicinare la palla alla porta
            mandare la palla ai colleghi o a se stessi
            evitare traiettorie da autogol
             */



            //if (!nogood)
            //if (m1.velocity_z >= 0)
            if (rob1.z < gz)
                if (dd < rules.BALL_RADIUS * 2)
                {
                    az[0].target_velocity_y = rules.ROBOT_MAX_JUMP_SPEED / 2;
                    az[0].jump_speed = rules.ROBOT_MAX_JUMP_SPEED / 2;
                }

            //if (nogood) De("!"); else De("");
            //if (pause > 1) De("p " + pause);
            
            Vec rob2 = game.robot[0, firstrobot];
            double dd2 = rob2.Sub(ball).Len();

            if (rob2 == null) return az;
            double d2 = Dist(rob2.x, rob2.z, gx, gz);
            if (ball.vz == 0) ball.vz = 0.000001;
            ggx = gx;
            if (ball.vz < -0.1)
                ggx = gx + (ball.z < 0 ? ((ball.z + 40) / -ball.vz) * (ball.vx) : 0);
            //De("ballx "+ball.x.ToString("0.0") + " , ggx = "+ggx.ToString("0.0") + " vx " + ball.vx.ToString("0.0"));

            if (gx > 12.0) ggx = 12.0;
            if (gx < -12.0) ggx = -12.0;

            if (dd2 < 10 + game.ball.y)
            {
                varied = 0;
                tilted = 0;

                if (rob2.z > gz - rules.BALL_RADIUS)
                {
                    if (game.ball.vz < 0)
                        varied = +game.ball.vz * 2;
                    else
                        varied = -dd2;
                    if (Abs(rob2.x - gx) < rules.BALL_RADIUS * 2)
                        tilted = ((game.ball.vx < 0) ? 5 : -5);
                }
                double agx = gx + tilted;
                if (gx > 13) agx = 13; if (gx < -13) agx = -13;
                az[1] = ReachAction(rob2, agx, gz + varied, true);
            }
            else
                az[1] = ReachAction(rob2, ggx, -39, false);



            if (rob2.z < gz - rules.BALL_RADIUS)
                //if (m2.velocity_z >= 0)
                if (d2 < rules.BALL_RADIUS + 1)
                {
                    az[1].target_velocity_y = rules.ROBOT_MAX_JUMP_SPEED;
                    az[1].jump_speed = rules.ROBOT_MAX_JUMP_SPEED;
                }

            return az;


        }

        public static double Timenow()
        {
            return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        }

        public static double Abs(double a) => a < 0 ? -a : a;

        public static double chunk = 0.2;

        public static bool Advance(ref Sgame g, double time)
        {
            bool collided = false;
            double totaltime = 0;
            while (totaltime<time)
            {
                double sparetime = totaltime > chunk ? chunk : totaltime;
                
                Sgame copy = new Sgame(g,true);
                if (Update(ref g, sparetime, true))
                {
                    g = copy;
                    for (int i = 0; i < rules.TICKS_PER_SECOND * chunk; i++)
                        if (Tick(ref g, false)) collided = true;

                }
                
                totaltime += sparetime;
            }


            return collided;
        }
        
        public static bool Tick(ref Sgame g, bool untilCollision)
        {
            bool res = false;

            for (int i = 0; i < rules.MICROTICKS_PER_TICK; i++)
                if (Update(ref g, oneontickspersecond / rules.MICROTICKS_PER_TICK,untilCollision))
                {

                    if (untilCollision) return true;
                    //De("collision at " + i);
                    res = true;
                }

            return res;
        }

        public static bool Collide_vectors(ref Vec a, ref Vec b)
        {
            Vec deltap = b.Sub(a);
            double dist = deltap.Len();
            double penet = a.r + b.r - dist;
            if (penet <= 0) return false;

            a.m = a.r == rules.BALL_RADIUS ? rules.BALL_MASS : rules.ROBOT_MASS;
            b.m = b.r == rules.BALL_RADIUS ? rules.BALL_MASS : rules.ROBOT_MASS;

            double ccc = ((1 / a.m) + (1 / b.m));
            double k_a = (1 / a.m) / ccc;
            double k_b = (1 / b.m) / ccc;
            Vec norm = deltap.Normalize();
            a = a.Sub(norm.Mul(penet * k_a));
            b = b.Sub(norm.Mul(penet * k_b));
            double deltav = b.VelocityVec().Sub(a.VelocityVec()).Dot(norm) +
                b.rcs - a.rcs;
            if (deltav<0)
            {
                Vec impulse = norm.Mul(1 + averagehit * (-deltav));
                a.AddVelocity(impulse.Mul(k_a));
                b.RemVelocity(impulse.Mul(k_b));


            }


            return true;
        }

        public static bool CollideWithArena(ref Vec a,bool verb)
        {
            

            double rimbalzo;
            if (a.r == 1) rimbalzo = rules.ROBOT_ARENA_E;
            else rimbalzo = rules.BALL_ARENA_E;
            
            Dan dta = a.DanToArena();
            double penet = a.r - dta.distance;
            
                if (penet <= 0) return false;
            if (verb) De("penet=" + penet);

            a.DoAdd(dta.normal.Mul(penet));
            
            double vel = a.VelocityVec().Dot(dta.normal) - a.rcs;
            if (vel<0)
            {


                    Vec zs = dta.normal.Mul(-vel * (1 + rimbalzo));
                if (verb) De("vel added " + zs.x+" "+zs.y+" "+zs.z);
                a.AddVelocity(zs);
                return true;
            }
            return false;
        }


        public static bool Update(ref Sgame g,double delta_time, bool untilCollision)
        {
            bool res = false;

            
            //robot movements
            for (int j = 0; j < 2; j++)
                for (int i = 0; i < robotsPerPlayer; i++)
                    g.robot[j, i].Move(delta_time);
            //radium adjustment
            //move ball
            g.ball.Move(delta_time);



            //collide robots
            for (int j = 0; j < 2; j++)
                for (int i = 0; i < robotsPerPlayer; i++)
                    for (int j2 = 0; j2 < 2; j2++)
                        for (int i2 = 0; i2 < robotsPerPlayer; i2++)
                        if (!((i==i2)&&(j==j2)))
                        { 
                            if (Collide_vectors(ref g.robot[j,i],ref g.robot[j2,i2]))
                                {
                                    //robot collision
                                    res = true;
                                }

                        }
            //collide robot-ball and robot-arena
            for (int j = 0; j < 2; j++)
                for (int i = 0; i < robotsPerPlayer; i++)
                {
                    // robot-ball collision
                    //if (Collide_vectors(ref g.robot[j, i], ref g.ball))
                      //  res = true;

                    if (CollideWithArena(ref g.robot[j, i],false))
                    {
                        res = true;
                    }
                }
                    //ball collide with arena

                    if (CollideWithArena(ref g.ball, false))
            {
             //   De("collision: " + v);
             //  De("becomes  : " + ballon);
                res = true;
            }
            // goal check

            //nitro distances check
            
            //g.ball = ballon.ToBall();

            return res;
        }

        public static Player CopyPlayer(Player b)
        {
            Player p = new Player();
            p.id = b.id;
            p.me = b.me;
            p.score = b.score;
            p.strategy_crashed = b.strategy_crashed;
            return p;
        }

        public static Robot CopyRobot(Robot b)
        {
            Robot c = new Robot(); c.x = b.x; c.y = b.y; c.z = b.z;
            c.velocity_x = b.velocity_x; c.velocity_y = b.velocity_y; c.velocity_z = b.velocity_z;
            c.radius = rules.BALL_RADIUS; c.id = b.id;c.is_teammate = b.is_teammate;
            c.player_id = b.player_id;c.nitro_amount = b.nitro_amount;
            c.touch = b.touch; c.touch_normal_x = b.touch_normal_x;
            c.touch_normal_y = b.touch_normal_y; c.touch_normal_z = b.touch_normal_z;
            return c;
        }


        public static Ball CopyBall(Ball b)
        {
            Ball c = new Ball();
            c.x = b.x;
            c.y = b.y;
            c.z = b.z;
            c.velocity_x = b.velocity_x;
            c.velocity_y = b.velocity_y;
            c.velocity_z = b.velocity_z;
            c.radius = rules.BALL_RADIUS;
            return c;
        }

        public static NitroPack CopyNitro(NitroPack b)
        {
            NitroPack c = new NitroPack();
            c.id=b.id;
            c.x = b.x; c.y = b.y; c.z = b.z;
            c.nitro_amount=b.nitro_amount;
            c.respawn_ticks = b.respawn_ticks ;
            return c;
    }

        public static Game CopyGame(Game g)
        {
            Game j = new Game();
            j.current_tick = g.current_tick;
            j.ball = CopyBall(g.ball);

            j.robots = new Robot[robotsPerPlayer];
            for (int i=0; i<robotsPerPlayer; i++)
            j.robots[i] = CopyRobot(g.robots[i]);
            

            j.nitro_packs = new NitroPack[g.nitro_packs.Length];
            for (int i = 0; i < g.nitro_packs.Length; i++)
                j.nitro_packs[i] = CopyNitro(g.nitro_packs[i]);

            return j;
        }
        



        public void Act(Robot me, Rules rulese, Game gamee, Action action)
        {
            if ((turn == 0) && (robotsDone == 0))
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
                rules = rulese;
                ar = rulese.arena;
                InitConstants();
                firstrobot = me.id;
                if (me.id == 1)
                    firstenemyrobot = robotsPerPlayer+1;
                else firstenemyrobot = 1;

            }
            //
            if (robotsDone == 0)
            {
                start = Timenow();
                if (turn > 0)
                {
                    bool iscored = false, hescored = false;
                    if (game.players[0].score != gamee.players[0].score)
                        iscored = true;
                    if (game.players[1].score != gamee.players[1].score)
                        hescored = true;
                    if (hescored || iscored)
                    {
                        //GOAL RESET
                        pause = 1;
                        //De("score");
                    }

                }

                game = new Sgame(gamee);
                if (game.ball.m != rules.BALL_MASS)
                    Re("nno!");
                Sgame g3 = new Sgame(game,false);
                

                Advance(ref g3, 0.25);
                if (g3.ball.m != rules.BALL_MASS)
                    Re("nno2!");
                //for (int i = 0; i < 10; i++) Tick(ref g3,false);
                afterball1 = new Vec(g3.ball);
                Advance(ref g3, 0.35);
                //for (int i = 0; i < 10; i++) Tick(ref g3,false);
                afterball = new Vec(g3.ball);


                

                bal = new Vec(game.ball);
                dw = bal.DanToArena();
                Re("time is " + (Timenow() - start));
                acts = ChooseMoves();
            }

            if (me.id - firstrobot < acts.Length)
                if (me.id - firstrobot >= 0)
                {
                    action.target_velocity_x = acts[me.id - firstrobot].target_velocity_x;
                    action.target_velocity_y = acts[me.id - firstrobot].target_velocity_y;
                    action.target_velocity_z = acts[me.id - firstrobot].target_velocity_z;
                    action.jump_speed = acts[me.id - firstrobot].jump_speed;
                }
            robotsDone++;

            if (robotsDone == 2)
            {
                turn++;
                robotsDone = 0;
            }
            //int k = -9999999;
            //while (Timenow() - start < 20) if (k < 99999) k++; else k = -999999;
        }

        

        public string CustomRendering()
        {

            Vec m1 = game.robot[0,firstrobot-1];
            double az = sfals > 1 ? 0 : 1 - sfals;
            double bz = nogood ? 1.0 : 0.0;
            Vec hh = bal.Add(dw.ToVec());
            /*
            string R =
                "[  " +
                "{\"Line\": {\"x1\": " + bal.x +
                ",\"y1\": " + bal.y + ",\"z1\": " + bal.z + ",\"x2\": " + (hh.x) + ",\"y2\": " +
                hh.y + ",\"z2\": " + hh.z +
                ",\"width\": 4.0,\"r\": 1.0,\"g\": 1.0,\"b\": 1.0,\"a\": 1.0}},";
            if (HasValue(m1.x))
            R

                += "{    \"Sphere\":       {      \"x\": " + m1.x.ToString("0.0") +
                           ",      \"y\": " + m1.y.ToString("0.0") +
                           ",      \"z\": " + m1.z.ToString("0.0") +
                           ",      \"radius\": 2.0,      \"r\": "
                           + az.ToString("0.0") + ",      \"g\": 0.0,      \"b\": "
                           + bz.ToString("0.0") + ",      \"a\": 0.6    \n  } },";
            if (HasValue(afterball.x))
            R +=
                
                           "{    \"Sphere\":       {      \"x\": " + afterball.x.ToString("0.0") +
                           ",      \"y\": " + afterball.y.ToString("0.0") +
                           ",      \"z\": " + afterball.z.ToString("0.0") +
                           ",      \"radius\": 2.0,      \"r\": "
                           + 1.0 + ",      \"g\": 1.0,      \"b\": "
                           + 1.0 + ",      \"a\": 1.0    \n  } },";
                   if (HasValue(afterball1.x))
                R +=


                "{    \"Sphere\":       {      \"x\": " + afterball1.x.ToString("0.0") +
                ",      \"y\": " + afterball1.y.ToString("0.0") +
                ",      \"z\": " + afterball1.z.ToString("0.0") +
                ",      \"radius\": 2.0,      \"r\": "
                + 1.0 + ",      \"g\": 0.0,      \"b\": "
                + 1.0 + ",      \"a\": 1.0    \n  } },";
                   R+=



                           "{    \"Sphere\":       {      \"x\": " + ggx.ToString("0.0") +
          ",      \"y\": " + 1.0 +
          ",      \"z\": " + -40.0 +
          ",      \"radius\": 2.0,      \"r\": "
          + 1.0 + ",      \"g\": 0.0,      \"b\": "
          + 1.0 + ",      \"a\": 1.0    \n  } }"


          + ",{\"Text\":\""+ printout + " \"}]";*/
            string R = "";
            printout = "";
            return R;
        }
    }
}
