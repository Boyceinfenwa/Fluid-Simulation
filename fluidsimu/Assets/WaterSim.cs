using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class WaterSim : MonoBehaviour
{

    [System.Serializable]
    public struct Parameters
    {
        #pragma warning disable 0649
        public float radius;
        public float sRadius;
        public float sRadiusSq;
        public float rDensity;
        public float gravMultiplier;
        public float mass;
        public float visc;
        public float drag;
        #pragma warning restore 0649
    }





    public struct Particle 
    {
       // ISpatialHash3D SpatialHash;
        public Vector3 pos;
        public Vector3 vel;
        public Vector3 fPhysic;
        public Vector3 fHeading;

        public float density;
        public float pressure;

        public int paramID;
        public GameObject GameObject;

        public void Init(Vector3 position,int parameterID, GameObject go)
        {
            pos = position;
            paramID = parameterID;
            GameObject = go;
            vel = Vector3.zero;
            fPhysic = Vector3.zero;
            fHeading = Vector3.zero;
            density = 0.0f;
            pressure = 0.0f;
           // position = SpatialHash.GetPos();
            
        }

        public void ResetParams(int parameterID)
        {
            paramID = parameterID;
        }
    }

   
    private struct SPHColl
    {
        public Vector3 position;
        public Vector3 right;
        public Vector3 up;
        public Vector2 scale;

        public void Init(Transform transform)
        {
            position = transform.position;
            right = transform.right;
            up = transform.up;
            scale = new Vector2(transform.lossyScale.x / 2f, transform.lossyScale.y / 2f);
        }

    }



  
    

    private static Vector3 grav =  new Vector3(0.0f, -9.81f, 0.0f);
    private const float gasConst = 2000.0f;
    private const float dt = 0.0008f;
    private const float boundDamping = -0.5f;


    // Properties
    [Header("Particle")]
    public GameObject particle = null;

    [Header("Parameters")]
    public int paramID = 0;
    public Parameters[] parameters = null;

    [Header("Particle amount")]
    public int amount = 100;
    public int rows = 4;

    private Particle[] particles;
    

    //public  SpacialHash<Particle> spacialHash;
 //   public NewSpatialHash<Particle> newHash;
    
    //public int cellSize = 1;
    //public List<Particle> list;

   public GameObject[] enviroment;
    int wallID = 0;
    
    
    private void Start()
    {
       
        SpawnParticles();
        
    }

    private void Update()
    {
          
      CalculateDP();
      Calculateforces();
      Intergrate();
      CalculateCol();
      ApplyPos();

        // reset
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // switch parameters
        if (Input.GetKeyDown(KeyCode.Q))
        {   
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].ResetParams(paramID);
            }
            paramID++;
        }
        if (paramID >= parameters.Length)
        {
            paramID = 0;
        }

        MoveWall();
    }

    private void SpawnParticles()
    {
        particles = new Particle[amount];
      //  spacialHash = new SpacialHash<Particle>(cellSize);
        
        
        for (int i = 0; i<amount; i++)
        {
            float jitter = (Random.value * 2f - 1f) * parameters[paramID].radius * 0.5f;

            float x = (i % rows) + (Random.Range(-0.1f, 0.1f));
            float y = 2 + (float)((i / rows) / rows) * 1.1f;
            float z = ((i / rows) % rows) + Random.Range(-0.1f, 0.1f);

            GameObject go = Instantiate(particle);
            
           // spacialHash.Insert(particles[i].pos, particles[i]);
            go.transform.parent = transform;
            go.transform.localScale = Vector3.one * parameters[paramID].radius;//Vector3.one * parameters[paramID].radius;
            go.transform.position = new Vector3(x + jitter, y, z + jitter);
            go.name = "FluidParticle" + i.ToString();
            

            particles[i].Init(new Vector3(x, y, z), paramID, go);
            
            
        }
    }

    static Vector3 DampVel(SPHColl coll, Vector3 vel, Vector3 peenNormal,float drag)
    {
        Vector3 newVel = Vector3.Dot(vel, peenNormal) * peenNormal * boundDamping + 
            Vector3.Dot(vel, coll.right) * coll.right * drag + 
            Vector3.Dot(vel, coll.up) * coll.up * drag;

        newVel = Vector3.Dot(newVel, Vector3.forward) * Vector3.forward + 
            Vector3.Dot(newVel, Vector3.right) * Vector3.right + 
            Vector3.Dot(newVel, Vector3.up) * Vector3.up;
        return newVel;
    }

    static bool Intersect(SPHColl coll, Vector3 pos, float radius, out Vector3 penNormal, out Vector3 penPos, out float PenLength )
    {
        Vector3 collProjection = coll.position - pos;

        penNormal = Vector3.Cross(coll.right, coll.up);
        PenLength = Mathf.Abs(Vector3.Dot(collProjection, penNormal)) - (radius / 2f);
        penPos = coll.position - collProjection;

        return PenLength < 0.0f && Mathf.Abs(Vector3.Dot(collProjection, coll.right)) < coll.scale.x && 
            Mathf.Abs(Vector3.Dot(collProjection, Vector3.up)) < coll.scale.y;
        
    }

    void Intergrate()
    {
        for (int i =0; i < particles.Length; i++)
        {
            particles[i].vel += dt * (particles[i].fPhysic) / particles[i].density;
            particles[i].pos += dt * (particles[i].vel);

        }
    }

    void Calculateforces()  // applying physics
    {
        for (int i = 0; i < particles.Length; i++)
        {
            Vector3 fPressure = Vector3.zero;
            Vector3 fVisc = Vector3.zero;

            for(int z = 0; z < particles.Length; z++)
            {
                if (i == z) continue;

                Vector3 rij = particles[z].pos - particles[i].pos;
                float r2 = rij.sqrMagnitude;
                float r = Mathf.Sqrt(r2);

                if (r < parameters[particles[i].paramID].sRadiusSq)
                {
                    fPressure += -rij.normalized * parameters[particles[i].paramID].mass * 
                        (particles[i].pressure + particles[z].pressure) / (2.0f * particles[z].density) * 
                        (-45.0f / (Mathf.PI * Mathf.Pow(parameters[particles[i].paramID].radius, 6.0f))) * 
                        Mathf.Pow(parameters[particles[i].paramID].radius - r, 2.0f);

                    fVisc += parameters[particles[i].paramID].visc * parameters[particles[i].paramID].mass *
                        (particles[z].vel - particles[i].vel) / particles[z].density * (45.0f / (Mathf.PI * 
                        Mathf.Pow(parameters[particles[i].paramID].sRadius, 6.0f))) * (parameters[particles[i].paramID].sRadius - r);
                }

            }
            
            Vector3 fGravity = grav * particles[i].density * parameters[particles[i].paramID].gravMultiplier;

            particles[i].fPhysic = fPressure + fVisc + fGravity;
        }
    }

    void CalculateDP()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].density = 0.0f;
            for(int z =0; z < particles.Length; z++)
            {
                Vector3 riz = particles[z].pos - particles[i].pos;
                float r2 = riz.sqrMagnitude;

                if (r2 < parameters[particles[i].paramID].sRadiusSq)
                {
                    particles[i].density += parameters[particles[i].paramID].mass * 
                        (315.0f / (64.0f * Mathf.PI * Mathf.Pow(parameters[particles[i].paramID].sRadius, 9.0f))) 
                        * Mathf.Pow(parameters[particles[i].paramID].sRadius - r2, 3.0f);
                }
            }

            particles[i].pressure = gasConst * (particles[i].density - parameters[particles[i].paramID].rDensity);
        }
    }


    void CalculateCol() // collisions
    {
        GameObject[] collObj = GameObject.FindGameObjectsWithTag("SPHCollider");
        SPHColl[] colls = new SPHColl[collObj.Length];

        for(int i = 0; i < colls.Length; i++)
        {
            colls[i].Init(collObj[i].transform);
        }
        for(int i = 0; i < particles.Length; i++)
        {
            for (int n = 0; n < colls.Length; n++)
            {
                Vector3 PenNormal;
                Vector3 PenPos;
                float PenLength;
                
                
                if (Intersect(colls[n], particles[i].pos, parameters[particles[i].paramID].radius, out PenNormal, out PenPos, out PenLength))
                {
                    
                   // Debug.Log("intersecting");
                    particles[i].vel = DampVel(colls[n], particles[i].vel, PenNormal, 1.0f - parameters[particles[i].paramID].drag);
                    particles[i].pos = PenPos - PenNormal * Mathf.Abs(PenLength);
                    
                }
               
            }
        }
    }

    void ApplyPos()
    {
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].GameObject.transform.position = particles[i].pos;
        }
    }

    private void Reset()
    {
        SceneManager.LoadScene(0);
    }

    void MoveWall()
    {
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 pos;
            pos = enviroment[wallID].transform.position;
            pos.x += 10 *10 * dt;
            enviroment[wallID].transform.position = pos;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 pos;
            pos = enviroment[wallID].transform.position;
            pos.x -= 10 * 10 * dt;
            enviroment[wallID].transform.position = pos;
        }

    }


}

