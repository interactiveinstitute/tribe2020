using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Markov<T>
{
    SortedList<T, SortedList<T, float>> map = new SortedList<T, SortedList<T, float>>();
    T currentState = default(T);
    int count = -1;

    public void InsertState(T newStateKey)
    {
        //Add new state to map
        map.Add(newStateKey, new SortedList<T, float>());

        float probability = 0.0f;

        List<T> keys = new List<T>(map.Keys);
        foreach (T existingKey in keys)
        {
            //Add new state key to all states
            map[existingKey].Add(newStateKey, probability);

            //Add existing state keys to new state
            if (!newStateKey.Equals(existingKey))
            {
                map[newStateKey].Add(existingKey, probability);
            }
        }
    }

    public void InsertStates(List<T> stateKeys)
    {
        float probability = 1.0f / stateKeys.Count;

        foreach (T fromStateKey in stateKeys)
        {
            map.Add(fromStateKey, new SortedList<T, float>());
            foreach (T toStateKey in stateKeys)
            {
                map[fromStateKey].Add(toStateKey, probability);
            }
        }
    }

    public float GetT(T keyFromState)
    {
        return ProbabilityFunctions.getT(map.IndexOfKey(keyFromState), map.Keys.Count);
    }

    //One-to-one
    public void SetProbability(T keyFromState, T keyToState, float probability, bool normalize = true)
    {
        map[keyFromState][keyToState] = probability;
        if (normalize)
        {
            NormalizeProbabilities(keyFromState);
        }
    }

    //One-to-many
    public void SetProbability(T keyFromState, List<T> keyToStates, ProbabilityFunctions.ProbabilityFunction probabilityFunction, float[] parameters = null, bool normalize = true)
    {
        //float t = 0;
        //float step = 1.0f / (keyToStates.Count - 1);
        int id = 0;
        foreach (T keyToState in keyToStates)
        {
            SetProbability(keyFromState, keyToState, probabilityFunction(id, keyToStates.Count, parameters), false);
            //t += step;
            id++;
        }

        if (normalize)
        {
            NormalizeProbabilities(keyFromState);
        }
    }

    //One-to-all
    public void SetProbability(T keyFromState, ProbabilityFunctions.ProbabilityFunction probabilityFunction, float[] parameters = null, bool normalize = true)
    {
        SetProbability(keyFromState, new List<T>(map[keyFromState].Keys), probabilityFunction, parameters, normalize);
    }

    //Many-to-one
    public void SetProbability(List<T> keyFromStates, T keyToState, float probability, bool normalize = true)
    {
        foreach (T keyFromState in keyFromStates)
        {
            SetProbability(keyFromState, keyToState, probability, normalize);
        }
    }

    //Many-to-many
    public void SetProbability(List<T> keyFromStates, List<T> keyToStates, ProbabilityFunctions.ProbabilityFunction probabilityFunction, float[] parameters = null, bool normalize = true)
    {
        foreach (T keyFromState in keyFromStates) {
            SetProbability(keyFromState, keyToStates, probabilityFunction, parameters, normalize);
        }
    }

    //Many-to-all
    public void SetProbability(List<T> keyFromStates, ProbabilityFunctions.ProbabilityFunction probabilityFunction, float[] parameters = null, bool normalize = true)
    {
        foreach (T keyFromState in keyFromStates)
        {
            SetProbability(keyFromState, probabilityFunction, parameters, normalize);
        }
    }

    //All-to-all
    public void SetProbability(ProbabilityFunctions.ProbabilityFunction probabilityFunction, float[] parameters = null, bool normalize = true)
    {
        SetProbability(new List<T>(map.Keys), probabilityFunction, parameters, normalize);
    }

    public SortedList<T,float> GetProbabilities(T keyFromState)
    {
        return map[keyFromState];
    }

    //Log probabilities
    public void LogProbabilities()
    {
        foreach (T keyFromState in map.Keys)
        {
            float accum = 0.0f;
            string str = keyFromState + ": ";
            foreach (T keyToState in map[keyFromState].Keys)
            {
                str += keyToState + " " + map[keyFromState][keyToState] + "; ";
                accum += map[keyFromState][keyToState];
            }
            Debug.Log(str + ". Total: " + accum);
        }
    }

    public void NormalizeProbabilities(T keyFromState)
    {
        float accum = 0.0f;
        List<T> keysTo = new List<T>(map[keyFromState].Keys);
        foreach (T keyToState in keysTo)
        {
            accum += map[keyFromState][keyToState];
        }

        foreach (T keyToState in keysTo)
        {
            map[keyFromState][keyToState] /= accum;
        }
    }

    public T Restart()
    {
        count = 0;
        return currentState;
    }

    public T Restart(T initialState)
    {
        currentState = map.ContainsKey(initialState) ? initialState : GetRandomState();
        return Restart();
    }

    public T RestartRandom()
    {
        return Restart(GetRandomState());
    }

    public void End()
    {
        count = -1;
    }

    public T GetRandomState()
    {
        int id = Random.Range(0, map.Count);
        return map.Keys.ElementAt(id);
    }

    public T SetToNextState()
    {
        return SetToNextState(currentState);
    }

    public T SetToNextState(T keyFromState)
    {
        return SetToNextState(map[keyFromState]);
    }

    public T SetToNextState(SortedList<T, float> keyToStates)
    {
        float r = Random.value;
        float accum = 0.0f;

        foreach (T keyToState in keyToStates.Keys)
        {
            accum += keyToStates[keyToState];
            if (r <= accum)
            {
                currentState = keyToState;
                break;
            }
        }
        count++;
        return currentState;
    }

    public T SetToNextState(T[] keyFromStates, float[] weights)
    {
        if(keyFromStates.Length != weights.Length)
        {
            Debug.LogError("Key array length doesn't match weight array length");
            return currentState;
        }

        SortedList<T, float> keyToStates = new SortedList<T, float>();

        foreach (T keyToState in map[keyFromStates[0]].Keys)
        {
            float accum = 0.0f;
            int i = 0;
            foreach (T keyFromState in keyFromStates)
            {
                accum += map[keyFromState][keyToState] * weights[i];
                i++;
            }

            keyToStates.Add(keyToState, accum);
        }

        return SetToNextState(keyToStates);

    }

    public T GetCurrentState()
    {
        return currentState;
    }

    public void SetCurrentState(T state)
    {
        currentState = state;
    }

    public int GetCount()
    {
        return count;
    }

    public bool IsActive()
    {
        return count >= 0;
    }

}

public struct ProbabilityFunctions
{
    public delegate float ProbabilityFunction(int id, int count, float[] parameters = null);

    public static float getT(int id, int count)
    {
        return id / (float)(count - 1);
    }

    public static float even(int id, int count, float[] parameters = null)
    {
        return 1.0f / count;
    }

    public static float exp(int id, int count, float[] parameters = null)
    {
        float t = getT(id, count);
        return Mathf.Exp(t);
    }

    public static float gaussian(int id, int count, float[] parameters)
    {
        float t = getT(id, count);
        float mean = parameters[0];
        float standardDeviation = parameters[1];
        float variance = Mathf.Pow(standardDeviation, 2.0f);
        return Mathf.Exp(-Mathf.Pow(t - mean, 2.0f) / (2.0f * variance)) / Mathf.Sqrt(2 * variance * Mathf.PI);
    }

    public static float pow(int id, int count, float[] parameters)
    {
        float t = getT(id, count);
        return Mathf.Pow(t, parameters[0]);
    }

    
}
