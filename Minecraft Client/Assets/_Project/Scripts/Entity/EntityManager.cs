using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
	public GameObject TestEntity;
	public World World;

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
		var mob = Instantiate(TestEntity, transform).GetComponent<Mob>();
		mob.Type = pkt.Type;
		mob.World = World;
		mob.UUID = pkt.UUID;
		mob.ID = pkt.EntityID;
		Vector3 pos = new Vector3((float)pkt.X, (float)pkt.Y, (float)pkt.Z); ;
		mob.MinecraftPosition = pos;
		mob.SetRotation(Quaternion.Euler(pkt.Pitch, pkt.Yaw, 0));
		mob.HeadPitch = pkt.HeadPitch;
		mob.name = $"{mob.Type.ToString()} ID:{mob.ID} UUID:{mob.UUID.ToString()}";

		Debug.Log($"Spawned entity at {pos}");
	}

	public void DestroyAllEntities()
	{
		_entities.ForEach(e => Destroy(e));
		_entities.Clear();
	}
}
