using UnityEngine;
using System;
using System.Collections;

public interface IStateBehaviour
{

	StateEngine stateMachine {
		get;
	}
	                        
	
	Enum GetState ();

	void Initialize<T> ();

	void ChangeState (Enum newState);

	void ChangeState (Enum newState, StateTransition transition);

}
