﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAction
{
    void ReceiveActionOnTrigger(float damage);
}