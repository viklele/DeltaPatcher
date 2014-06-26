DeltaPatcher
============

A minimalist of how to implement a patch method, similar to the Patch from Delta of oData.

I started with Asp.netMVCPatchExample (https://github.com/rikvanmechelen/Asp.netMVCPatchExample), and realized that non generics based delta patcher will force using either static methods or using a standard base class. So, I decided to roll out a generics based patcher using DynamicObject. 

I have borrowed the NotPatchableAttribute marker attribute from Asp.netMVCPatchExample.  It gives you a flexibility to skip patching properties that you don't want to modify.

        public class Appointment : AbstractModel
        {
                [NotPatchable]
                public int ID { get; set; }

                [NotPatchable]
                public int DoctorId { get; set; }

                [NotPatchable]
                public int PatientId { get; set; }

                public Status AppointmentStatus { get; set; }

                public DateTime ScheduleAt { get; set; }

                public int Duration { get; set; }
        }


I am caching PropInfo of patchable properties to improve performance when you repeatedly use the patcher for same type of object.

Usage of this patcher is exactly same as OData's Delta patcher. Here is a sample code:

      [HttpPatch]
      [Route("api/appointments/{appointmentId}")]
      public HttpResponseMessage UpdateAppointment(Guid appointmentId, [FromBody]DeltaPatcher<Appointment> delta)
      {
          Appointment appointment = AppointmentManager.GetById(appointmentId);
          delta.Patch(appointment);

          AppointmentManager.Save(appointment);

          return Request.CreateResponse(HttpStatusCode.OK, appointment);
      }
