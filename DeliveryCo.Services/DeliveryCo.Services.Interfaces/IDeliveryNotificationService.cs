﻿using System;
using System.ServiceModel;

namespace DeliveryCo.Services.Interfaces
{
    public enum DeliveryInfoStatus { Submitted, Delivered, Failed }

    [ServiceContract]
    public interface IDeliveryNotificationService
    {
        [OperationContract]
        void NotifyDeliveryCompletion(Guid pDeliveryId, DeliveryInfoStatus status);

        [OperationContract]
        bool NotifyPickedUpOrder(Guid pDeliveryId);

        [OperationContract]
        bool NotifyOnDeliveryTruckOrder(Guid pDeliveryId);
    }
}
