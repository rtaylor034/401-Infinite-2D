
/// <summary>
/// [Notifying Delegate]
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Methods of this delegate type should always return a <see langword="new"/> <typeparamref name="T"/> instance.
/// <br></br>
/// > Actual constructed type may inherit[ : ] from <typeparamref name="T"/>, but must be casted.
/// </remarks>
public delegate T ConstructionTemplate<T> () where T : class;