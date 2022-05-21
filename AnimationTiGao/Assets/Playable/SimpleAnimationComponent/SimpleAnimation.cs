using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public partial class SimpleAnimation: MonoBehaviour
{
    public interface State
    {
        bool enabled { get; set; }
        bool isValid { get; }
        float time { get; set; }
        float normalizedTime { get; set; }
        float speed { get; set; }
        string name { get; set; }
        float weight { get; set; }
        float length { get; }
        AnimationClip clip { get; }
        WrapMode wrapMode { get; set; }

    }
    public Animator animator
    {
        get
        {
            if (m_Animator == null)
            {
                m_Animator = GetComponent<Animator>();
            }
            return m_Animator;
        }
    }

    //animator更新模式
    public bool animatePhysics
    {
        get { return m_AnimatePhysics; }
        set { m_AnimatePhysics = value; animator.updateMode = m_AnimatePhysics ? AnimatorUpdateMode.AnimatePhysics : AnimatorUpdateMode.Normal; }
    }
    
    //animator剔除模式
    public AnimatorCullingMode cullingMode
    {
        get { return animator.cullingMode; }
        set { m_CullingMode = value;  animator.cullingMode = m_CullingMode; }
    }

    public bool isPlaying { get { return m_PlayableBehaviour.IsPlaying(); } }

    public bool playAutomatically
    {
        get { return m_PlayAutomatically; }
        set { m_PlayAutomatically = value; }
    }

    public AnimationClip clip
    {
        get { return m_Clip; }
        set
        {
            LegacyClipCheck(value);
            m_Clip = value;
        }  
    }

    public WrapMode wrapMode
    {
        get { return m_WrapMode; }
        set { m_WrapMode = value; }
    }

    public void AddClip(AnimationClip clip, string newName)
    {
        LegacyClipCheck(clip);
        AddState(clip, newName);
    }

    public void Blend(string stateName, float targetWeight, float fadeLength)
    {
        m_Animator.enabled = true;
        Kick();
        m_PlayableBehaviour.Blend(stateName, targetWeight,  fadeLength);
    }

    public void CrossFade(string stateName, float fadeLength)
    {
        m_Animator.enabled = true;
        Kick();
        m_PlayableBehaviour.Crossfade(stateName, fadeLength);
    }

    public void CrossFadeQueued(string stateName, float fadeLength, QueueMode queueMode)
    {
        m_Animator.enabled = true;
        Kick();
        m_PlayableBehaviour.CrossfadeQueued(stateName, fadeLength, queueMode);
    }

    public int GetClipCount()
    {
        return m_PlayableBehaviour.GetClipCount();
    }

    public bool IsPlaying(string stateName)
    {
        return m_PlayableBehaviour.IsPlaying(stateName);
    }

    public void Stop()
    {
        m_PlayableBehaviour.StopAll();
    }

    public void Stop(string stateName)
    {
        m_PlayableBehaviour.Stop(stateName);
    }

    public void Sample()
    {
        m_Graph.Evaluate();
    }

    public bool Play()
    {
        m_Animator.enabled = true;
        Kick();
        if (m_Clip != null && m_PlayAutomatically)
        {
            m_PlayableBehaviour.Play(kDefaultStateName);
        }
        return false;
    }

    public void AddState(AnimationClip clip, string name)
    {
        LegacyClipCheck(clip);
        Kick();
        //调用自定义的playable方法
        if (m_PlayableBehaviour.AddClip(clip, name))
        {
            RebuildStates();
        }
        
    }

    public void RemoveState(string name)
    {
        if (m_PlayableBehaviour.RemoveClip(name))
        {
            RebuildStates();
        }
    }
    
    //TODO
    public bool Play(string stateName)
    {
        m_Animator.enabled = true;
        Kick();
        return m_PlayableBehaviour.Play(stateName);
    }
    
    //TODO
    public void PlayQueued(string stateName, QueueMode queueMode)
    {
        m_Animator.enabled = true;
        Kick();
        m_PlayableBehaviour.PlayQueued(stateName, queueMode);
    }

    public void RemoveClip(AnimationClip clip)
    {
        if (clip == null)
            throw new System.NullReferenceException("clip");

        if ( m_PlayableBehaviour.RemoveClip(clip) )
        {
            RebuildStates();
        }
       
    }

    public void Rewind()
    {
        Kick();
        m_PlayableBehaviour.Rewind();
    }

    public void Rewind(string stateName)
    {
        Kick();
        m_PlayableBehaviour.Rewind(stateName);
    }

    public State GetState(string stateName)
    {
        SimpleAnimationPlayable.IState state = m_PlayableBehaviour.GetState(stateName);
        if (state == null)
            return null;

        return new StateImpl(state, this);
    }

    public IEnumerable<State> GetStates()
    {
        return new StateEnumerable(this);
    }

    public State this[string name]
    {
        get { return GetState(name); }
    }

}
