using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
	private readonly List<Entity> _entities = new List<Entity>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	/// <summary>
	/// Spawns a mob in the server
	/// </summary>
	/// <param name="pkt"></param>
	public void SpawnMob(SpawnMobPacket pkt)
	{
		
	}

	public void DestroyAllEntities()
	{
		_entities.ForEach(e => Destroy(e));
		_entities.Clear();
	}
}
