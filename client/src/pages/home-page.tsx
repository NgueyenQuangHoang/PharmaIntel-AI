import { HeroSection } from '@/features/home/components/HeroSection';
import { StatsSection } from '@/features/home/components/StatsSection';
import { FeaturedMedications } from '@/features/home/components/FeaturedMedications';
import { AIInsightsCallout } from '@/features/home/components/AIInsightsCallout';

export function HomePage() {
  return (
    <>
      <HeroSection />
      <StatsSection />
      <FeaturedMedications />
      <AIInsightsCallout />
    </>
  );
}
