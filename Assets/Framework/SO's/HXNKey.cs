using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HXN Key", menuName = "Technical/HXN Key")]
public class HXNKey : ScriptableObject
{
    [SerializeField]
    private Hex[] _hexPrefabs;
    [SerializeField]
    private char[] _keys;

    private Dictionary<char, Hex> _charKeyDict = new();

    private void OnValidate()
    {
        _charKeyDict.Clear();

        for (int i = 0; i < _hexPrefabs.Length; i++)
        {
            _charKeyDict.Add(_keys[i], _hexPrefabs[i]);
        }

        //debug
        foreach (var pair in _charKeyDict)
        {
            Debug.Log($"{pair.Key} => {pair.Value.name}");
        }
    }

    public Hex GetHex(char key)
    {
        if (_charKeyDict.TryGetValue(key, out Hex o)) return o;
        return null;
    }

}
