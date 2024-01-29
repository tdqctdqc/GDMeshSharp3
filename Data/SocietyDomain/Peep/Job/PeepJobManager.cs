// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using Godot;
//
// public class PeepJobManager : IModelManager<PeepJob>
// {
//     public static PeepJob Farmer { get; private set; } 
//             = new PeepJob(nameof(Farmer));
//     public static PeepJob Prole { get; private set; } 
//         = new PeepJob(nameof(Prole));
//     public static PeepJob Miner { get; private set; } 
//         = new PeepJob(nameof(Miner));
//     public static PeepJob Bureaucrat { get; private set; } 
//         = new PeepJob(nameof(Bureaucrat));
//     public static PeepJob Builder { get; private set; } 
//         = new PeepJob(nameof(Builder));
//     public static PeepJob Unemployed { get; private set; } 
//         = new PeepJob(nameof(Unemployed));
//     public Dictionary<string, PeepJob> Models { get; set; }
//
//     public PeepJobManager()
//     {
//         Models = GetType().GetStaticPropertiesOfType<PeepJob>().ToDictionary(pj => pj.Name, pj => pj);
//     }
//
// }