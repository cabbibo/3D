using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Structs {
  public struct Bone {
    public Matrix4x4 transform;
    public Matrix4x4 bindPose;
  }

  public struct SkinnedVertex {
    public float used;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 nor;
    public Vector3 tan;
    public Vector2 uv;

    public Vector3 targetPos;

    public Vector3 bindPos;
    public Vector3 bindNor;
    public Vector3 bindTan;

    public Vector4 boneWeights;
    public Vector4 boneIDs;
    public Vector3 debug;
  }

  public struct Vertex {
    public float used;
    public Vector3 pos;
    public Vector3 vel;
    public Vector3 nor;
    public Vector2 uv;

    public Vector3 targetPos;
    public Vector3 debug;
  }

  public struct Hand {
    public Matrix4x4 localToWorld;
    public Matrix4x4 worldToLocal;
    public Vector3 pos;
    public Vector3 vel;
    public float trigger;
    public Vector3 debug;
  }

  public struct Head {
    public Matrix4x4 localToWorld;
    public Matrix4x4 worldToLocal;
    public Vector3 pos;
    public Vector3 debug;
  }

  public struct Human {
    public Head head;
    public Hand hand1;
    public Hand hand2;
  }

  public struct Transfer {
    public Vector3 vertex;
    public Vector3 normal;
  }

  public static int GetSizeOf(Type obj) {
    return Marshal.SizeOf (obj);
  }

}