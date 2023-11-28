using System;

namespace Editor {
  public static class Utils {
    private static readonly object lockObject_ = new object();
    private static ulong lastTimestamp_ = 0;
    private static ulong counter_ = 0;

    public static int GetId() {
      return (int)DateTime.UtcNow.Subtract(new DateTime(2023, 1, 1)).TotalSeconds;
    }

    public static ulong GenerateUniqueId() {
      lock (lockObject_) {
        ulong timestamp = (ulong)(DateTime.UtcNow.Ticks / 10);
        if (timestamp > lastTimestamp_) {
          lastTimestamp_ = timestamp;
          counter_ = 0;
        }
        else {
          counter_++;
        }

        if (counter_ > 0x3FFFFF) {
          while (timestamp <= lastTimestamp_) {
            timestamp = (ulong)(DateTime.UtcNow.Ticks / 10);
          }

          lastTimestamp_ = timestamp;
          counter_ = 0;
        }

        ulong uniqueID = ((timestamp & 0x1FFFFFFFFFF) << 22) | (counter_ & 0x3FFFFF);
        return uniqueID;
      }
    }
  }
}