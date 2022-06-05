namespace KAG.Shared
{
	public interface IEntityPool
	{
		Entity Acquire(ushort id);
		void Return(Entity entity);
	}
}