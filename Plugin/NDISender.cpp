#include <cstdint>
#include <Processing.NDI.Lib.h>
#include <TargetConditionals.h>

namespace
{
    // NDI sender class
    class Sender
    {
    public:

        Sender(const char* name)
        {
            NDIlib_send_create_t create(name, nullptr, false);
            instance_ = NDIlib_send_create(&create);
        }

        ~Sender()
        {
            NDIlib_send_destroy(instance_);
        }

        void sendFrame(void* data, int width, int height, uint32_t fourCC)
        {
            static NDIlib_video_frame_v2_t frame;

            frame.xres = width;
            frame.yres = height;
            frame.FourCC = static_cast<NDIlib_FourCC_type_e>(fourCC);
#if TARGET_OS_OSX
            frame.frame_format_type = NDIlib_frame_format_type_interleaved;
#endif
            frame.p_data = static_cast<uint8_t*>(data);
            frame.line_stride_in_bytes = width * 2;

            NDIlib_send_send_video_async_v2(instance_, &frame);
        }

        void synchronize()
        {
            NDIlib_send_send_video_async_v2(instance_, nullptr);
        }

    private:

        NDIlib_send_instance_t instance_;
    };
}

extern "C" Sender *NDI_CreateSender(const char* name)
{
    return new Sender(name);
}

extern "C" void NDI_DestroySender(Sender* sender)
{
    delete sender;
}

extern "C" void NDI_SendFrame(Sender* sender, void* data, int width, int height, uint32_t fourCC)
{
    sender->sendFrame(data, width, height, fourCC);
}

extern "C" void NDI_SyncSender(Sender* sender)
{
    sender->synchronize();
}
